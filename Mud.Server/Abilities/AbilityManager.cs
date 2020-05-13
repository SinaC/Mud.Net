using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Settings;

namespace Mud.Server.Abilities
{
    public partial class AbilityManager : IAbilityManager
    {
        private IRandomManager RandomManager { get; }
        private ISettings Settings { get; }
        private IWorld World { get; }
        private ITimeManager TimeManager { get; }

        private readonly List<IAbility> _abilities;

        private readonly Dictionary<string, IAbility> _abilitiesByName;

        public AbilityManager(IRandomManager randomManager, ISettings settings, IWorld world, ITimeManager timeManager)
        {
            RandomManager = randomManager;
            Settings = settings;
            World = world;
            TimeManager = timeManager;

            _abilities = new List<IAbility>();

            // Reflection to gather every methods with Spell/Skill attributes
            var abilityInfos = typeof(AbilityManager).GetMethods(BindingFlags.Instance | BindingFlags.Public).Select(m => new {method = m, attribute = m.GetCustomAttribute(typeof(AbilityAttribute), false) as AbilityAttribute })
                .Where(x => x.attribute != null);
            foreach (var abilityInfo in abilityInfos)
            {
                AbilityKinds kind = AbilityKinds.Passive;
                if (abilityInfo.attribute is SpellAttribute)
                    kind = AbilityKinds.Spell;
                else if (abilityInfo.attribute is SkillAttribute)
                    kind = AbilityKinds.Skill;
                IAbility ability = new Ability(kind, abilityInfo.attribute, abilityInfo.method);
                _abilities.Add(ability);
            }
            // Reflection gather every list of passives
            var passiveLists = typeof(AbilityManager).GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => new { property = p, attribute = p.GetCustomAttribute(typeof(PassiveListAttribute), false) })
                .Where(x => x.attribute != null);
            foreach (var passiveList in passiveLists)
            {
                var getResult = passiveList.property.GetValue(this);
                if (getResult is IEnumerable<IAbility> passives)
                {
                    foreach (IAbility passive in passives)
                        _abilities.Add(passive);
                }
                else
                    Log.Default.WriteLine(LogLevels.Warning, "Found [PassiveList] attribute on {0} but it does not seem to be a list of abilities", getResult);
            }

            // Check duplicates
            var duplicateIds = _abilities.GroupBy(x => x.Id).Where(g => g.Count() > 1).Select(x => x.Key);
            foreach (int duplicateId in duplicateIds)
                Log.Default.WriteLine(LogLevels.Error, "Duplicate ability id {0}", duplicateId);
            var duplicateNames = _abilities.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(x => x.Key);
            foreach (string duplicateName in duplicateNames)
                Log.Default.WriteLine(LogLevels.Error, "Duplicate ability name {0}", duplicateName);

            // Build abilities by name
            _abilitiesByName = _abilities.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First(), StringComparer.InvariantCultureIgnoreCase); // group by to remove duplicate
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

        public IAbility this[int id] => _abilities.SingleOrDefault(x => x.Id == id);

        public IEnumerable<IAbility> Spells => _abilities.Where(x => x.Kind == AbilityKinds.Spell);
        public IEnumerable<IAbility> Skills => _abilities.Where(x => x.Kind == AbilityKinds.Skill);
        public IEnumerable<IAbility> Passives => _abilities.Where(x => x.Kind == AbilityKinds.Passive);

        public CastResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                caster.Send("Cast which what where?");
                return CastResults.MissingParameter;
            }

            IPlayableCharacter pcCaster = caster as IPlayableCharacter;

            // 1) search spell
            KnownAbility knownAbility = Search(caster.KnownAbilities, caster.Level, x => x.Kind == AbilityKinds.Spell, parameters[0]); // filter on spell
            if (knownAbility == null)
            {
                caster.Send("You don't know any spells of that name.");
                return CastResults.InvalidParameter;
            }

            // 2) strip first argument
            (rawParameters, parameters) = CommandHelpers.SkipParameters(parameters, 1);

            // 3) get target
            IEntity target;
            AbilityTargetResults targetResult = GetAbilityTarget(knownAbility.Ability, caster, out target, rawParameters, parameters);
            if (targetResult != AbilityTargetResults.Ok)
                return MapCastResultToCommandExecutionResult(targetResult);

            // 4) check cooldown
            // TODO
            //int cooldownSecondsLeft = caster.CooldownSecondsLeft(ability);
            //if (cooldownSecondsLeft > 0)
            //{
            //    caster.Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
            //    return false;
            //}

            // 5) check resource costs
            int? cost = null;
            if (knownAbility.ResourceKind.HasValue && knownAbility.CostAmount > 0 && knownAbility.CostAmountOperator != CostAmountOperators.None)
            {
                ResourceKinds resourceKind = knownAbility.ResourceKind.Value;
                if (!caster.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                {
                    caster.Send("You can't use {0} as resource for the moment.", knownAbility.ResourceKind);
                    return CastResults.CantUseRequiredResource;
                }
                int resourceLeft = caster[resourceKind];
                switch (knownAbility.CostAmountOperator)
                {
                    case CostAmountOperators.Fixed:
                        cost = knownAbility.CostAmount;
                        break;
                    case CostAmountOperators.Percentage:
                        cost = caster.MaxResource(resourceKind) * knownAbility.CostAmount / 100;
                        break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Unexpected CostAmountOperator {0}", knownAbility.CostAmountOperator);
                        cost = 100;
                        break;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                {
                    caster.Send("You don't have enough {0}.", knownAbility.ResourceKind);
                    return CastResults.NotEnoughResource;
                }
            }

            // 6) check if failed
            var abilityLearnInfo = caster.GetLearnInfo(knownAbility.Ability);
            if (!RandomManager.Chance(abilityLearnInfo.learned))
            {
                caster.Send("You lost your concentration.");
                pcCaster?.CheckAbilityImprove(knownAbility, false, 1);
                // pay half resource
                if (cost.HasValue && cost.Value > 1)
                    caster.UpdateResource(knownAbility.ResourceKind.Value, -cost.Value / 2);
                return CastResults.Failed;
            }

            // 7) pay resource
            if (cost.HasValue && cost.Value >= 1)
                caster.UpdateResource(knownAbility.ResourceKind.Value, -cost.Value);

            // 8) say spell if not ventriloquate
            if (!StringCompareHelpers.StringEquals(knownAbility.Ability.Name, "ventriloquate"))
                SayAbility(knownAbility.Ability, caster);

            // 9) invoke spell
            InvokeSpell(knownAbility.Ability, caster.Level, caster, target, rawParameters, parameters);

            // 10) GCD
            pcCaster?.ImpersonatedBy?.SetGlobalCooldown(knownAbility.Ability.PulseWaitTime);

            // 11) check improve true
            pcCaster?.CheckAbilityImprove(knownAbility, true, knownAbility.Ability.LearnDifficultyMultiplier);

            // 12) if aggressive: multi hit if still in same room
            INonPlayableCharacter npcVictim = target as INonPlayableCharacter;
            if ((knownAbility.Ability.Target == AbilityTargets.CharacterOffensive
                || knownAbility.Ability.Target == AbilityTargets.CharacterFighting
                || (knownAbility.Ability.Target == AbilityTargets.ItemHereOrCharacterOffensive && target is ICharacter))
                && target != caster
                && npcVictim?.ControlledBy != caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == target && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(caster);
                        break;
                    }
                }
            }

            //
            return CastResults.Ok;
        }

        public CastResults CastFromItem(IAbility ability, int level, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (ability == null)
                return CastResults.InvalidParameter;

            // 1) check if target is compatible
            AbilityTargetResults targetResult = GetItemAbilityTarget(ability, caster, ref target);
            if (targetResult != AbilityTargetResults.Ok)
                return MapCastResultToCommandExecutionResult(targetResult);

            // 2) invoke spell
            InvokeSpell(ability, level, caster, target, rawParameters, parameters);

            // 3) if aggressive: multi hit if still in same room
            INonPlayableCharacter npcVictim = target as INonPlayableCharacter;
            if ((ability.Target == AbilityTargets.CharacterOffensive
                || ability.Target == AbilityTargets.CharacterFighting
                || (ability.Target == AbilityTargets.ItemHereOrCharacterOffensive && target is ICharacter))
                && target != caster
                && npcVictim?.ControlledBy != caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == target && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(caster);
                        break;
                    }
                }
            }

            //
            return CastResults.Ok;
        }

        public UseResults Use(IAbility ability, ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            IPlayableCharacter pcUser = user as IPlayableCharacter;
            
            // 1) check if it's a skill
            if (ability == null || ability.Kind != AbilityKinds.Skill)
                return UseResults.Error;

            // 2) get target
            IEntity target;
            AbilityTargetResults targetResult = GetAbilityTarget(ability, user, out target, rawParameters, parameters);
            if (targetResult != AbilityTargetResults.Ok)
                return MapUseResultToCommandExecutionResult(targetResult);

            // 3) invoke skill
            var abilityLearnInfo = user.GetLearnInfo(ability);
            object rawResult = InvokeSkill(ability, abilityLearnInfo.learned, user, target, rawParameters, parameters);
            UseResults result = rawResult is UseResults results
                ? results
                : UseResults.Error;

            // 4) GCD
            pcUser?.ImpersonatedBy?.SetGlobalCooldown(ability.PulseWaitTime);

            // 5) improve skill
            if (result == UseResults.Ok || result == UseResults.Failed)
                pcUser?.CheckAbilityImprove(abilityLearnInfo.knownAbility, result == UseResults.Ok, ability.LearnDifficultyMultiplier);

            //
            return result;
        }

        public AbilityTargetResults GetAbilityTarget(IAbility ability, ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = null;
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    break;
                case AbilityTargets.CharacterOffensive:
                    if (parameters.Length < 1)
                    {
                        target = caster.Fighting;
                        if (target == null)
                        {
                            caster.Send("Cast the spell on whom?");
                            return AbilityTargetResults.MissingParameter;
                        }
                    }
                    else
                        target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
                    if (target == null)
                    {
                        caster.Send("They aren't here.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    ICharacter victim = (ICharacter)target;
                    if (caster is IPlayableCharacter)
                    {
                        if (caster != target && victim.IsSafe(caster))
                        {
                            caster.Send("Not on that target.");
                            return AbilityTargetResults.InvalidTarget;
                        }
                        // TODO: check_killer
                    }
                    if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm) && victim.ControlledBy == caster)
                    {
                        caster.Send("You can't do that on your own follower.");
                        return AbilityTargetResults.InvalidTarget;
                    }
                    // victim found
                    break;
                case AbilityTargets.CharacterDefensive:
                    if (parameters.Length < 1)
                        target = caster;
                    else
                    {
                        target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
                        if (target == null)
                        {
                            caster.Send("They aren't here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim found
                    break;
                case AbilityTargets.CharacterSelf:
                    if (parameters.Length >= 1)
                    {
                        ICharacter search = FindHelpers.FindByName(caster.Room.People, parameters[0]);
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
                    if (parameters.Length < 1)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
                    if (target == null)
                    {
                        caster.Send("You are not carrying that.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    // item found
                    break;
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    if (parameters.Length < 1)
                    {
                        target = caster.Fighting;
                        if (target == null)
                        {
                            caster.Send("Cast the spell on whom or what?");
                            return AbilityTargetResults.MissingParameter;
                        }
                    }
                    else
                        target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
                    if (target != null)
                    {
                        // TODO: check if safe/charm/...   messages.TargetIsSafe
                    }
                    else // character not found, search item in room, in inventor, in equipment
                    {
                        target = FindHelpers.FindItemHere(caster, parameters[0]);
                        if (target == null)
                        {
                            caster.Send("You don't see that here.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // victim or item (target) found
                    break;
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    target = parameters.Length < 1
                        ? caster
                        : FindHelpers.FindByName(caster.Room.People, parameters[0]);
                    if (target == null)
                    {
                        target = FindHelpers.FindByName(caster.Inventory, parameters[0]);
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
                    if (parameters.Length >= 1)
                    {
                        target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
                        if (target == null)
                        {
                            caster.Send("You are not carrying that.");
                            return AbilityTargetResults.TargetNotFound;
                        }
                    }
                    // item found
                    break;
                case AbilityTargets.ArmorInventory:
                    if (parameters.Length < 1)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
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
                    if (parameters.Length < 1)
                    {
                        caster.Send("What should the spell be cast upon?");
                        return AbilityTargetResults.MissingParameter;
                    }
                    target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
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
                case AbilityTargets.CharacterFighting:
                    target = caster.Fighting;
                    if (target == null)
                    {
                        caster.Send("You aren't fighting anyone.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    break;
                case AbilityTargets.CharacterWorldwide:
                    target = FindHelpers.FindChararacterInWorld(caster, parameters[0]);
                    if (target == null)
                    {
                        caster.Send("You failed.");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "GetAbilityTarget: unexpected AbilityTarget {0}", ability.Target);
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
                    if (caster != target && (target as ICharacter)?.IsSafe(caster) == true)
                    {
                        caster.Send("Not on that target.");
                        return AbilityTargetResults.InvalidTarget;
                    }
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

                    if (target is ICharacter victim && victim != caster && victim.IsSafeSpell(caster, false))
                    {
                        caster.Send("Somehting isn't right...");
                        return AbilityTargetResults.InvalidTarget;
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
                case AbilityTargets.CharacterFighting:
                    target = caster.Fighting;
                    if (target == null)
                    {
                        caster.Send("But you aren't fighting anyone!");
                        return AbilityTargetResults.TargetNotFound;
                    }
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "GetItemAbilityTarget: unexpected AbilityTarget {0}", ability.Target);
                    return AbilityTargetResults.Error;
            }

            return AbilityTargetResults.Ok;
        }

        public KnownAbility Search(IEnumerable<KnownAbility> knownAbilities, int level, Func<IAbility, bool> abilityFilterFunc, CommandParameter parameter)
        {
            return knownAbilities.Where(x =>
                    abilityFilterFunc(x.Ability)
                    && x.Level <= level // high level enough
                    && x.Learned > 0 // practice at least once
                    && StringCompareHelpers.StringStartsWith(x.Ability.Name, parameter.Value))
                .ElementAtOrDefault(parameter.Count - 1);
        }

        #endregion

        private object InvokeSpell(IAbility ability, int level, ICharacter caster, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (ability == null)
                return null;
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster });
                case AbilityTargets.CharacterOffensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.CharacterDefensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.CharacterSelf:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.ItemInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.Custom:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, rawParameters, parameters });
                case AbilityTargets.OptionalItemInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.ArmorInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.WeaponInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.CharacterFighting:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
                case AbilityTargets.CharacterWorldwide:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, level, caster, target });
            }
            return null;
        }

        private object InvokeSkill(IAbility ability, int learned, ICharacter source, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (ability == null)
                return null;
            switch (ability.Target)
            {
                case AbilityTargets.None:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source });
                case AbilityTargets.CharacterOffensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.CharacterDefensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.CharacterSelf:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source });
                case AbilityTargets.ItemInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.ItemHereOrCharacterOffensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.ItemInventoryOrCharacterDefensive:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.Custom:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, rawParameters, parameters });
                case AbilityTargets.OptionalItemInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.ArmorInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.WeaponInventory:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.CharacterFighting:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target });
                case AbilityTargets.CharacterWorldwide:
                    return ability.MethodInfo.Invoke(this, new object[] { ability, learned, source, target }); // TODO: should never happen
            }
            return null;
        }

        private static readonly Dictionary<string, string> SyllableTable = new Dictionary<string, string> // TODO: use Trie ?
        {
            { " ",      " "     },
            { "ar",     "abra"      },
            { "au",     "kada"      },
            { "bless",  "fido"      },
            { "blind",  "nose"      },
            { "bur",    "mosa"      },
            { "cu",     "judi"      },
            { "de",     "oculo"     },
            { "en",     "unso"      },
            { "light",  "dies"      },
            { "lo",     "hi"        },
            { "mor",    "zak"       },
            { "move",   "sido"      },
            { "ness",   "lacri"     },
            { "ning",   "illa"      },
            { "per",    "duda"      },
            { "ra",     "gru"       },
            { "fresh",  "ima"       },
            { "re",     "candus"    },
            { "son",    "sabru"     },
            { "tect",   "infra"     },
            { "tri",    "cula"      },
            { "ven",    "nofo"      },
            { "a", "a" }, { "b", "b" }, { "c", "q" }, { "d", "e" },
            { "e", "z" }, { "f", "y" }, { "g", "o" }, { "h", "p" },
            { "i", "u" }, { "j", "y" }, { "k", "t" }, { "l", "r" },
            { "m", "w" }, { "n", "i" }, { "o", "a" }, { "p", "s" },
            { "q", "d" }, { "r", "f" }, { "s", "g" }, { "t", "h" },
            { "u", "j" }, { "v", "z" }, { "w", "x" }, { "x", "n" },
            { "y", "l" }, { "z", "k" }
        };

        // TODO: maybe a table should be constructed for each spell to avoid computing at each cast
        private void SayAbility(IAbility ability, ICharacter source)
        {
            if (ability == null)
                return;
            if (ability.Kind == AbilityKinds.Spell)
            {
                source.Send("You cast '{0}'.", ability.Name);

                // Build mystical words for spell
                StringBuilder mysticalWords = new StringBuilder();
                string abilityName = ability.Name.ToLowerInvariant();
                string remaining = abilityName;
                while (remaining.Length > 0)
                {
                    bool found = false;
                    foreach (var syllable in SyllableTable)
                    {
                        if (remaining.StartsWith(syllable.Key))
                        {
                            mysticalWords.Append(syllable.Value);
                            remaining = remaining.Substring(syllable.Key.Length);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        mysticalWords.Append('?');
                        remaining = remaining.Substring(1);
                        Log.Default.WriteLine(LogLevels.Warning, "Spell {0} contains a character which is not found in syllable table", ability.Name);
                    }
                }

                // Say to people in room except source
                foreach (ICharacter target in source.Room.People.Where(x => x != source))
                {
                    if (target.KnownAbilities.Any(x => x.Ability == ability && x.Level < target.Level))
                        target.Act(ActOptions.ToCharacter, "{0} casts the spell '{1}'.", source, ability.Name);
                    else
                    {

                        target.Act(ActOptions.ToCharacter, "{0} utters the words, '{1}'.", source, mysticalWords);
                    }
                }
            }
            else if (ability.Kind == AbilityKinds.Skill)
            {
                source.Send("You use '{0}'.", ability.Name);
                source.Act(ActOptions.ToRoom, "{0} uses '{1}'.", source, ability.Name);
            }
            else
            {
                source.Send("You use '{0}'.", ability.Name);
                source.Act(ActOptions.ToRoom, "{0} uses '{1}'.", source, ability.Name);
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} has unknown type {1}!", ability.Name, ability.Kind);
            }
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

        private UseResults MapUseResultToCommandExecutionResult(AbilityTargetResults result)
        {
            {
                switch (result)
                {
                    case AbilityTargetResults.MissingParameter:
                        return UseResults.MissingParameter;
                    case AbilityTargetResults.InvalidTarget:
                        return UseResults.InvalidTarget;
                    case AbilityTargetResults.TargetNotFound:
                        return UseResults.TargetNotFound;
                    case AbilityTargetResults.Error:
                        return UseResults.Error;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Unexpected AbilityTargetResults {0}", result);
                        return UseResults.Error;
                }
            }
        }

        private static IAbility Passive(int id, string name, AbilityFlags flags = AbilityFlags.None) => new Ability(AbilityKinds.Passive, id, name, AbilityTargets.None, 0, flags, null, null, null, 0);

    }
}
