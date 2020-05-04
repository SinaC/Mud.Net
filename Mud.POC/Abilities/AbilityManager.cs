using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Domain;

namespace Mud.POC.Abilities
{
    public class AbilityManager : IAbilityManager
    {
        private IRandomManager RandomManager { get; }

        private readonly List<IAbility> _abilities;

        private readonly Dictionary<string, IAbility> _abilitiesByName;

        public AbilityManager(IRandomManager randomManager)
        {
            RandomManager = randomManager;

            _abilities = new List<IAbility>();

            // Reflection to gather every methods with Spell/Skill attributes
            var abilityInfos = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public), (t, m) => new { type = t, method = m, attribute = m.GetCustomAttribute(typeof(AbilityAttribute), false) as AbilityAttribute })
                .Where(x => x.attribute != null);
            foreach (var abilityInfo in abilityInfos)
            {
                IAbility ability = new Ability(new AbilityMethodInfo(abilityInfo.attribute, abilityInfo.method));
                //if (abilityInfo.attribute.GenerateCommand)
                //{
                //    CommandAttribute ca = new CommandAttribute(abilityInfo.attribute.Name)
                //    {
                //        Categories = new[] { "Skill" },
                //        Hidden = false,
                //        Priority = 1,
                //        AddCommandInParameters = true // !! this is mandatory
                //    };
                //    Func<string, CommandParameter[], bool> func = (rawParameters, parameters) => AbilityManager.Process(this, parameters);
                //    CommandMethodInfo cmi = new CommandMethodInfo(ca, func.Method);
                //}
                _abilities.Add(ability);
            }
            // Add passive abilities
            foreach (IAbility passive in Passives.Abilities)
                _abilities.Add(passive);

            // Build abilities by name
            _abilitiesByName = new Dictionary<string, IAbility>(_abilities.ToDictionary(x => x.Name, x => x), StringComparer.InvariantCultureIgnoreCase);
        }

        #region IAbilityManager

        public IEnumerable<IAbility> Abilities => _abilities;

        public IAbility this[string name]
        {
            get
            {
                _abilitiesByName.TryGetValue(name, out IAbility ability);
                return ability;
            }
        }

        public CastResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                caster.Send("Cast which what where?");
                return CastResults.MissingParameter;
            }

            IPlayableCharacter pcCaster = caster as IPlayableCharacter;

            // 1) search spell
            KnownAbility knownAbility = Search(caster.KnownAbilities, caster.Level, x => !x.AbilityFlags.HasFlag(AbilityFlags.Passive), parameters[0]); // filter on non-passive
            if (knownAbility == null)
            {
                caster.Send("You don't know any spells of that name.");
                return CastResults.InvalidParameter;
            }

            // 2) get target
            IEntity target;
            AbilityTargetResults targetResult = GetAbilityTarget(knownAbility.Ability, caster, out target, rawParameters, parameters);
            if (targetResult != AbilityTargetResults.Ok)
                return MapCastResultToCommandExecutionResult(targetResult);

            // 3) check cooldown
            // TODO
            //int cooldownSecondsLeft = caster.CooldownSecondsLeft(ability);
            //if (cooldownSecondsLeft > 0)
            //{
            //    caster.Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
            //    return false;
            //}

            // 4) check resource costs
            int? cost = null;
            if (knownAbility.ResourceKind != ResourceKinds.None && knownAbility.CostAmount > 0 && knownAbility.CostAmountOperator != CostAmountOperators.None)
            {
                if (!caster.CurrentResourceKinds.Contains(knownAbility.ResourceKind)) // TODO: not sure about this test
                {
                    caster.Send("You can't use {0} as resource for the moment.", knownAbility.ResourceKind);
                    return CastResults.CantUseRequiredResource;
                }
                int resourceLeft = caster[knownAbility.ResourceKind];
                if (knownAbility.CostAmountOperator == CostAmountOperators.Fixed)
                    cost = knownAbility.CostAmount;
                else //ability.CostType == CostAmountOperators.Percentage
                {
                    int maxResource = caster.GetMaxResource(knownAbility.ResourceKind);
                    cost = maxResource * knownAbility.CostAmount / 100;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                {
                    caster.Send("You don't have enough {0}.", knownAbility.ResourceKind);
                    return CastResults.NotEnoughResource;
                }
            }

            // 5) check if failed
            if (!RandomManager.Chance(knownAbility.Learned))
            {
                caster.Send("You lost your concentration.");
                pcCaster?.CheckAbilityImprove(knownAbility, false, 1);
                // pay half resource
                if (cost.HasValue && cost.Value > 1)
                    caster.UpdateResource(knownAbility.ResourceKind, -cost.Value / 2);
                return CastResults.Failed;
            }

            //6) pay resource
            if (cost.HasValue)
                caster.UpdateResource(knownAbility.ResourceKind, -cost.Value);

            // TODO: 7) say spell if not ventriloquate

            // 8) invoke spell
            InvokeAbility(knownAbility.Ability, caster.Level, caster, target, rawParameters, parameters);

            // TODO: 9) set GCD
            // 10) check improve true
            pcCaster?.CheckAbilityImprove(knownAbility, true, 1);

            // TODO: 11) if aggressive: multi hit if still in same room

            return CastResults.Ok;
        }

        public CastResults CastFromItem(IAbility ability, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            // 1) check if target is compatible
            AbilityTargetResults targetResult = GetItemAbilityTarget(ability, caster, ref target);
            if (targetResult != AbilityTargetResults.Ok)
                return MapCastResultToCommandExecutionResult(targetResult);

            // 2) invoke spell
            InvokeAbility(ability, caster.Level, caster, target, rawParameters, parameters);

            // TODO: 3) if aggressive: multi hit if still in same room

            return CastResults.Ok;
        }

        public AbilityTargetResults GetAbilityTarget(IAbility ability, ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = null;
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    break;
                case AbilityTargets.CharacterOffensive:
                    if (parameters.Length < 2)
                    {
                        target = caster.Fighting;
                        if (target == null)
                        {
                            caster.Send("Cast the spell on whom?");
                            return AbilityTargetResults.MissingParameter;
                        }
                    }
                    else
                    {
                        target = FindByName(caster.Room.People, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("They aren't here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim found
                    // TODO: check if safe/charm/...   messages.TargetIsSafe
                    break;
                case AbilityTargets.CharacterDefensive:
                    if (parameters.Length < 2)
                        target = caster;
                    else
                    {
                        target = FindByName(caster.Room.People, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("They aren't here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim found
                    break;
                case AbilityTargets.CharacterSelf:
                    if (parameters.Length >= 2)
                    {
                        ICharacter search = FindByName(caster.Room.People, parameters[1]);
                        if (search != caster)
                        {
                            caster.Send("You cannot cast this spell on another.");
                            return AbilityTargetResults.InvalidTarget;
                        }
                    }
                    target = caster;
                    // victim found
                    break;
                case AbilityTargets.ItemInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    // item found
                    break;
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    if (parameters.Length < 2)
                    {
                        target = caster.Fighting;
                        if (target == null)
                        {
                            caster.Send("Cast the spell on whom or what?");
                            return AbilityTargetResults.MissingParameter;
                        }
                    }
                    else
                        target = FindByName(caster.Room.People, parameters[1]);
                    if (target != null)
                    {
                        // TODO: check if safe/charm/...   messages.TargetIsSafe
                    }
                    else // character not found, search item in room, in inventor, in equipment
                    {
                        target = FindItemHere(caster, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("You don't see that here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim or item (target) found
                    break;
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    if (parameters.Length < 2)
                        target = caster;
                    else
                        target = FindByName(caster.Room.People, parameters[1]);
                    if (target == null)
                    {
                        target = FindByName(caster.Inventory, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("You don't see that here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim or item (target) found
                    break;
                case AbilityTargets.Custom:
                    break;
                case AbilityTargets.OptionalItemInventory:
                    if (parameters.Length >= 2)
                    {
                        target = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                        if (target == null)
                        {
                            caster.Send("You are not carrying that.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // item found
                    break;
                case AbilityTargets.ArmorInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    if (!(target is IItemArmor))
                    {
                        target = null;
                        caster.Send("That isn't an armor.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    // item found
                    break;
                case AbilityTargets.WeaponInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    if (!(target is IItemWeapon))
                    {
                        target = null;
                        caster.Send("That isn't an weapon.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    // item found
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected AbilityTarget {0}", ability.Target);
                    return AbilityTargetResults.Error;
            }
            return AbilityTargetResults.Ok;
        }

        public AbilityTargetResults GetItemAbilityTarget(IAbility ability, ICharacter caster, ref IEntity target)
        {
            if (target is IRoom)
            {
                Log.Default.WriteLine(LogLevels.Error, "AbilityManager.GetItemAbilityTarget: preselected target was IRoom.");
                return AbilityTargetResults.Error;
            }

            switch (ability.Target)
            {
                case AbilityTargets.None:
                    break;
                case AbilityTargets.CharacterOffensive:
                    if (target == null)
                        target = caster.Fighting;
                    if (target == null)
                    {
                        caster.Send("You can't do that.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    // TODO: check if safe -> Something isn't right...
                    break;
                case AbilityTargets.CharacterDefensive:
                    if (target == null)
                        target = caster;
                    break;
                case AbilityTargets.CharacterSelf:
                    if (target == null)
                        target = caster;
                    break;
                case AbilityTargets.ItemInventory:
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    if (!(target is IItem))
                    {
                        caster.Send("You can't do that.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    break;
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    if (target == null)
                    {
                        if (caster.Fighting != null)
                            target = caster.Fighting;
                        else
                        {
                            caster.Send("You can't do that.");
                            return AbilityTargetResults.InvalidTarget;
                        }
                    }

                    if (target is ICharacter victim)
                    {
                        // TODO: check if safe -> Something isn't right...
                    }
                    break;
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    if (target == null)
                        target = caster;
                    break;
                case AbilityTargets.Custom:
                    break;
                case AbilityTargets.OptionalItemInventory:
                    if (target != null && !(target is IItem))
                    {
                        caster.Send("You can't do that.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    break;
                case AbilityTargets.ArmorInventory:
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    if (!(target is IItemArmor))
                    {
                        caster.Send("You can't do that.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    break;
                case AbilityTargets.WeaponInventory:
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    if (!(target is IItemWeapon))
                    {
                        caster.Send("You can't do that.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected AbilityTarget {0}", ability.Target);
                    return AbilityTargetResults.Error;
            }

            return AbilityTargetResults.Ok;
        }

        #endregion

        private void InvokeAbility(IAbility ability, int level, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster });
                    break;
                case AbilityTargets.CharacterOffensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.CharacterDefensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.CharacterSelf:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.ItemInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.Custom:
                    if (parameters.Length > 1)
                    {
                        var newParameters = CommandHelpers.SkipParameters(parameters, 1);
                        ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, newParameters.rawParameters });
                    }
                    else
                        ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, string.Empty });
                    break;
                case AbilityTargets.OptionalItemInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.ArmorInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
                case AbilityTargets.WeaponInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                    break;
            }
        }

        private KnownAbility Search(IEnumerable<KnownAbility> knownAbilities, int level, Func<IAbility, bool> abilityFilterFunc, CommandParameter parameter)
        {
            return knownAbilities.Where(x =>
                abilityFilterFunc(x.Ability)
                && x.Level <= level // high level enough
                && x.Learned > 0 // practice at least once
                && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameter.Value))
                .ElementAtOrDefault(parameter.Count - 1);
        }

        private CastResults MapCastResultToCommandExecutionResult(AbilityTargetResults result)
        {
            {
                switch (result)
                {
                    case AbilityTargetResults.MissingParameter:
                        return CastResults.MissingParameter;
                    case AbilityTargetResults.InvalidTarget:
                        return CastResults.InvalidTarget;
                    case AbilityTargetResults.TargetNotFound:
                        return CastResults.TargetNotFound;
                    case AbilityTargetResults.Error:
                        return CastResults.Error;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Unexpected AbilityTargetResults {0}", result);
                        return CastResults.Error;
                }
            }
        }

        // TODO: use FindHelpers.
        private T FindByName<T>(IEnumerable<T> list, CommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringListsEquals(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringCompareHelpers.StringListsStartsWith(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
        }

        private IItem FindItemHere(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        {
            return FindByName(
                character.Room.Content
                    .Concat(character.Inventory)
                    .Concat(character.Equipments),
                parameter, perfectMatch);
        }
    }
}
