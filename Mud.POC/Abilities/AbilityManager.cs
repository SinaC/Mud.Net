using Mud.Container;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.Abilities
{
    public class AbilityManager : IAbilityManager
    {
        private IRandomManager RandomManager { get; }

        private List<IAbility> _abilities;

        public AbilityManager(IRandomManager randomManager)
        {
            RandomManager = randomManager;

            _abilities = new List<IAbility>();

            // Reflection to gather every methods with Spell/Skill attributes
            var abilityInfos = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public), (t, m) => new { type = t, method = m, attribute = m.GetCustomAttribute(typeof(AbilityAttribute), false) as AbilityAttribute})
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

            // TODO: check uniqueness of ability (name and id)
        }

        public IEnumerable<IAbility> Abilities => _abilities;

        public IAbility this[string name] => _abilities.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));

        public CommandExecutionResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                caster.Send("Cast which what where?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // 1) search spell
            KnownAbility knownAbility = Search(caster.KnownAbilities, caster.Level, parameters[0]);
            if (knownAbility == null)
            {
                caster.Send("You don't know any spells of that name.");
                return CommandExecutionResults.InvalidParameter;
            }

            // 2) get target
            ICharacter victim = null;
            IItem item = null;
            IEntity target = null;
            CommandExecutionResults getAbilityTargetResult = GetAbilityTarget(knownAbility.Ability.Target, caster, out victim, out item, out target, rawParameters, parameters);
            if (getAbilityTargetResult != CommandExecutionResults.Ok)
                return getAbilityTargetResult;

            // 3) check cooldown
            // TODO
            //int cooldownSecondsLeft = caster.CooldownSecondsLeft(ability);
            //if (cooldownSecondsLeft > 0)
            //{
            //    caster.Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
            //    return false;
            //}

            // 4) check resource costs
            // TODO: cost depends on NPC/Class/Level/...
            //int cost = ability.CostAmount; // default value (always overwritten if significant)
            //if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != CostAmountOperators.None)
            //{
            //    if (!caster.CurrentResourceKinds.Contains(ability.ResourceKind)) // TODO: not sure about this test
            //    {
            //        caster.Send("You can't use {0} as resource for the moment.", ability.ResourceKind);
            //        return false;
            //    }
            //    int resourceLeft = source[ability.ResourceKind];
            //    if (ability.CostType == CostAmountOperators.Fixed)
            //        cost = ability.CostAmount;
            //    else //ability.CostType == CostAmountOperators.Percentage
            //    {
            //        int maxResource = caster.GetMaxResource(ability.ResourceKind);
            //        cost = maxResource * ability.CostAmount / 100;
            //    }
            //    bool enoughResource = cost <= resourceLeft;
            //    if (!enoughResource)
            //    {
            //        caster.Send("You don't have enough {0}.", ability.ResourceKind);
            //        return false;
            //    }
            //}

            // 5) check if failed
            if (!RandomManager.Chance(knownAbility.Learned))
            {
                caster.Send("You lost your concentration.");
                // TODO: check improve false
                // TODO: pay half resource
                return CommandExecutionResults.NoExecution;
            }

            // TODO: 6) pay resource
            //if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != CostAmountOperators.None)
            //    source.UpdateResource(ability.ResourceKind, -cost);

            // TODO: 7) say spell if not ventriloquate

            // 8) invoke spell
            InvokeAbility(knownAbility.Ability, caster.Level, caster, victim, item, target, rawParameters, parameters);
            
            // TODO: 9) set GCD
            // TODO: 10) check improve true
            // TODO: 11) if aggressive: multi hit if still in same room

            return CommandExecutionResults.Ok;
        }

        private CommandExecutionResults GetAbilityTarget(AbilityTargets abilityTarget, ICharacter caster, out ICharacter victim, out IItem item, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = null;
            victim = null;
            item = null;
            switch (abilityTarget)
            {
                case AbilityTargets.None:
                    break;
                case AbilityTargets.CharacterOffensive:
                    if (parameters.Length < 2)
                    {
                        victim = caster.Fighting;
                        if (victim == null)
                        {
                            caster.Send("Cast the spell on whom?");
                            return CommandExecutionResults.SyntaxErrorNoDisplay;
                        }
                    }
                    else
                    {
                        victim = FindByName(caster.Room.People, parameters[1]);
                        if (victim == null)
                        {
                            caster.Send("They aren't here.");
                            return CommandExecutionResults.TargetNotFound;
                        }
                    }
                    // victim found
                    // TODO: check if safe/charm/...
                    break;
                case AbilityTargets.CharacterDefensive:
                    if (parameters.Length < 2)
                        victim = caster;
                    else
                    {
                        victim = FindByName(caster.Room.People, parameters[1]);
                        if (victim == null)
                        {
                            caster.Send("They aren't here.");
                            return CommandExecutionResults.TargetNotFound;
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
                            return CommandExecutionResults.InvalidTarget;
                        }
                    }
                    victim = caster;
                    // victim found
                    break;
                case AbilityTargets.ItemInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return CommandExecutionResults.SyntaxErrorNoDisplay;
                    }
                    item = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (item == null)
                    {
                        caster.Send("You are not carrying that.");
                        return CommandExecutionResults.TargetNotFound;
                    }
                    // item found
                    break;
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    if (parameters.Length < 2)
                    {
                        target = caster.Fighting;
                        if (target == null)
                        {
                            caster.Send("Cast the spell on whom?");
                            return CommandExecutionResults.SyntaxErrorNoDisplay;
                        }
                    }
                    else
                        target = FindByName(caster.Room.People, parameters[1]);
                    if (target != null)
                    {
                        // TODO: check if safe/charm/...
                    }
                    else // character not found, search item in room, in inventor, in equipment
                    {
                        target = FindItemHere(caster, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("You don't see that here.");
                            return CommandExecutionResults.TargetNotFound;
                        }
                    }
                    // victim or item (target) found
                    break;
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    if (parameters.Length < 2)
                        target = caster;
                    else
                        target = FindByName(caster.Room.People, parameters[1]);
                    if (target != null)
                    {
                        // TODO: check if safe/charm/...
                    }
                    else // character not found, search item in room, in inventor, in equipment
                    {
                        target = FindByName(caster.Inventory, parameters[1]);
                        if (target == null)
                        {
                            caster.Send("You don't see that here.");
                            return CommandExecutionResults.TargetNotFound;
                        }
                    }
                    // victim or item (target) found
                    break;
                case AbilityTargets.Custom:
                    break;
                case AbilityTargets.OptionalItemInventory:
                    if (parameters.Length >= 2)
                    {
                        item = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                        if (item == null)
                        {
                            caster.Send("You are not carrying that.");
                            return CommandExecutionResults.TargetNotFound;
                        }
                    }
                    // item found
                    break;
                case AbilityTargets.ArmorInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return CommandExecutionResults.SyntaxErrorNoDisplay;
                    }
                    item = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (item == null)
                    {
                        caster.Send("You are not carrying that.");
                        return CommandExecutionResults.TargetNotFound;
                    }
                    if (!(item is IItemArmor))
                    {
                        caster.Send("That isn't an armor.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    // item found
                    break;
                case AbilityTargets.WeaponInventory:
                    if (parameters.Length < 2)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return CommandExecutionResults.SyntaxErrorNoDisplay;
                    }
                    item = FindByName(caster.Inventory, parameters[1]); // TODO: equipments ?
                    if (item == null)
                    {
                        caster.Send("You are not carrying that.");
                        return CommandExecutionResults.TargetNotFound;
                    }
                    if (!(item is IItemWeapon))
                    {
                        caster.Send("That isn't an armor.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    // item found
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected AbilityTarget {0}", abilityTarget);
                    return CommandExecutionResults.Error;
            }
            return CommandExecutionResults.Ok;
        }

        private void InvokeAbility(IAbility ability, int level, ICharacter caster, ICharacter victim, IItem item, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster });
                    break;
                case AbilityTargets.CharacterOffensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, victim });
                    break;
                case AbilityTargets.CharacterDefensive:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, victim });
                    break;
                case AbilityTargets.CharacterSelf:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, victim });
                    break;
                case AbilityTargets.ItemInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, item });
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
                        ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, string.Empty});
                    break;
                case AbilityTargets.OptionalItemInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, item });
                    break;
                case AbilityTargets.ArmorInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, item });
                    break;
                case AbilityTargets.WeaponInventory:
                    ability.AbilityMethodInfo.MethodInfo.Invoke(this, new object[] { ability, level, caster, item });
                    break;
            }
        }

        private KnownAbility Search(IEnumerable<KnownAbility> knownAbilities, int level, CommandParameter parameter)
        {
            return knownAbilities.Where(x =>
                !x.Ability.AbilityFlags.HasFlag(AbilityFlags.Passive)
                && x.Level <= level
                && x.Learned > 0
                && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameter.Value))
                .ElementAtOrDefault(parameter.Count - 1);
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
