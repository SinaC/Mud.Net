using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    public class AbilityManager : IAbilityManager
    {
        private const int WeakenedSoulAbilityId = 999;
        private const int ParryAbilityId = 1000;
        private const int DodgeAbilityId = 1001;
        private const int ShieldBlockAbilityId = 1002;

        private readonly List<IAbility> _abilities = new List<IAbility> // TODO: dictionary on id + Trie on name
        {
            // Linked to Power Word: Shield (cannot be used/casted)
            new Ability(WeakenedSoulAbilityId, "Weakened Soul", AbilityTargets.Target, ResourceKinds.None, AmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.CannotBeUsed),
            //
            new Ability(100, "Wrath", AbilityTargets.Target, ResourceKinds.Mana, AmountOperators.Percentage, 4, 1, 0, 0, SchoolTypes.Nature, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageAbilityEffect(149, ComputedAttributeTypes.SpellPower, SchoolTypes.Nature)),
            new Ability(101, "Trash", AbilityTargets.Room, ResourceKinds.Energy, AmountOperators.Fixed, 50, 1, 0, 15, SchoolTypes.Physical, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.None, new DamageAbilityEffect(513, ComputedAttributeTypes.AttackPower, SchoolTypes.Physical), new DotAbilityEffect(365, ComputedAttributeTypes.SpellPower, SchoolTypes.Physical, 3)),
            new Ability(102, "Shadow Word: Pain", AbilityTargets.Target, ResourceKinds.Mana, AmountOperators.Percentage, 1, 1, 0, 18, SchoolTypes.Shadow, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new DamageAbilityEffect(475, ComputedAttributeTypes.SpellPower, SchoolTypes.Shadow), new DotAbilityEffect(475, ComputedAttributeTypes.SpellPower, SchoolTypes.Shadow, 3)),
            new Ability(103, "Rupture", AbilityTargets.Target, ResourceKinds.Energy, AmountOperators.Fixed, 25, 1, 0, 8/* TODO: multiplied by combo*/, SchoolTypes.Physical, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.None, new DotAbilityEffect(685, ComputedAttributeTypes.AttackPower, SchoolTypes.Physical, 2)),
            new Ability(104, "Renew", AbilityTargets.TargetOrSelf, ResourceKinds.Mana, AmountOperators.Percentage, 2, 1, 0, 12, SchoolTypes.Holy, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new HealAbilityEffect(22, ComputedAttributeTypes.SpellPower), new HotAbilityEffect(44, ComputedAttributeTypes.SpellPower, 3)),
            new Ability(105, "Power Word: Shield", AbilityTargets.TargetOrSelf, ResourceKinds.Mana, AmountOperators.Percentage, 2, 1, 6, 15, SchoolTypes.Holy, AbilityMechanics.Shielded, DispelTypes.Magic, AbilityFlags.None, new PowerWordShieldEffect()),
            new Ability(106, "Death Coil", AbilityTargets.TargetOrSelf, ResourceKinds.Runic, AmountOperators.Fixed, 30, 1, 0, 30, SchoolTypes.Shadow, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageOrHealEffect(0.88f, 0.88f*5, ComputedAttributeTypes.AttackPower, SchoolTypes.Shadow), new AuraAbilityEffect(AuraModifiers.MaxHitPoints, 3, AmountOperators.Percentage)),
            new Ability(107, "Berserking", AbilityTargets.Self, ResourceKinds.None, AmountOperators.None, 0, 1, 3*60, 10, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackSpeed, 15, AmountOperators.Percentage)),
            new Ability(108, "Battle Shout", AbilityTargets.Group, ResourceKinds.None, AmountOperators.None, 0, 1, 0, 1*60*60, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackPower, 10, AmountOperators.Percentage)),

            new Ability(ParryAbilityId, "Parry", AbilityTargets.Self, ResourceKinds.None, AmountOperators.None, 0, 0, 0, 0, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            new Ability(DodgeAbilityId, "Dodge", AbilityTargets.Self, ResourceKinds.None, AmountOperators.None, 0, 0, 0, 0, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            new Ability(ShieldBlockAbilityId, "Shield Block", AbilityTargets.Self, ResourceKinds.None, AmountOperators.None, 0, 0, 0, 0, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
        };

        #region Singleton

        private static readonly Lazy<AbilityManager> Lazy = new Lazy<AbilityManager>(() => new AbilityManager());

        public static IAbilityManager Instance => Lazy.Value;

        private AbilityManager()
        {
            WeakenedSoulAbility = this[WeakenedSoulAbilityId];
            ParryAbility = this[ParryAbilityId];
            DodgerAbility = this[DodgeAbilityId];
            ShieldBlockAbility = this[ShieldBlockAbilityId];
        }

        #endregion

        #region IAbilityManager

        public IAbility WeakenedSoulAbility { get; }
        public IAbility ParryAbility { get; }
        public IAbility DodgerAbility { get; }
        public IAbility ShieldBlockAbility { get; }

        public IReadOnlyCollection<IAbility> Abilities => _abilities.AsReadOnly();

        public IAbility this[int id]
        {
            get { return _abilities.FirstOrDefault(x => x.Id == id); }
        }

        public IAbility this[string name]
        {
            get { return _abilities.FirstOrDefault(x => FindHelpers.StringEquals(x.Name,name)); }
        }

        public IAbility Search(CommandParameter parameter, bool includePassive = false)
        {
            return _abilities.Where(x =>
                (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed
                && (!includePassive || (x.Flags & AbilityFlags.Passive) != AbilityFlags.Passive)
                && FindHelpers.StringStartWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            //0/ Search ability (in known abilities)
            if (parameters.Length == 0)
            {
                source.Send("Cast/Use what ?" + Environment.NewLine);
                return false;
            }
            IAbility ability = Search(source.KnownAbilities, source.Level, parameters[0]);
            if (ability == null)
            {
                source.Send("You don't know any abilities of that name" + Environment.NewLine);
                return false;
            }
            //1/ Check cooldown
            int cooldownSecondsLeft = source.CooldownSecondsLeft(ability);
            if (cooldownSecondsLeft > 0)
            {
                source.Send("{0} is in cooldown for {1}." + Environment.NewLine, ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
                return false;
            }
            //2/ Check resource
            int cost = ability.CostAmount; // default value (always overwritten if significant)
            if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != AmountOperators.None)
            {
                int resourceLeft = source[ability.ResourceKind];
                if (ability.CostType == AmountOperators.Fixed)
                    cost = ability.CostAmount;
                else //ability.CostType == AmountOperators.Percentage
                {
                    int maxResource = source.GetMaxResource(ability.ResourceKind);
                    cost = maxResource*ability.CostAmount/100;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                {
                    source.Send("You don't have enough {0}."+Environment.NewLine, ability.ResourceKind);
                    return false;
                }
            }
            //3/ Check target(s)
            List<ICharacter> targets;
            switch (ability.Target)
            {
                case AbilityTargets.Self:
                    targets = new List<ICharacter> {source};
                    break;
                case AbilityTargets.Target:
                {
                    if (parameters.Length == 1)
                    {
                        source.Send("{0} on whom ?" + Environment.NewLine, ability.Name);
                        return false;
                    }
                    ICharacter target = FindHelpers.FindByName(source.Room.People, parameters[1]);
                    if (target == null)
                    {
                        source.Send(StringHelpers.CharacterNotFound);
                        return false;
                    }
                    targets = new List<ICharacter> {target};
                    break;
                }
                case AbilityTargets.TargetOrSelf:
                {
                    if (parameters.Length == 1)
                        targets = new List<ICharacter> {source};
                    else
                    {
                        ICharacter target = FindHelpers.FindByName(source.Room.People, parameters[1]);
                        if (target == null)
                        {
                            source.Send(StringHelpers.CharacterNotFound);
                            return false;
                        }
                        targets = new List<ICharacter> {target};
                    }
                    break;
                }
                case AbilityTargets.Group:
                {
                    // Source + group members
                    targets = new List<ICharacter>(source.GroupMembers)
                    {
                        source
                    };
                    break;
                }
                case AbilityTargets.Room:
                {
                    targets = source.Room.People.Where(x => x != source && !source.GroupMembers.Contains(x)).ToList();
                    break;
                }
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown target {0} for ability {1}[{2}]!!!", ability.Target, ability.Name, ability.Id);
                    targets = Enumerable.Empty<ICharacter>().ToList();
                    break;
            }
            //4/ Say ability
            source.Send("You cast/use '{0}'." + Environment.NewLine, ability.Name); // TODO: better wording
            //5/ Pay resource cost
            if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != AmountOperators.None)
                source.SpendResource(ability.ResourceKind, cost);
            //6/ Perform effect(s) on target(s)
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(targets);
            foreach (ICharacter target in clone)
            {
                if (source != target)
                    target.Act(ActOptions.ToCharacter, "{0} casts/uses '{1}' on you.", source, ability.Name); // TODO: better wording
                ProcessOnOneTarget(source, target, ability);
            }
            //7/ Set global cooldown
            source.ImpersonatedBy?.SetGlobalCooldown(ability.GlobalCooldown);
            // TODO: if ability cannot be used because an effect cannot be casted (ex. power word: shield with weakened soul is still affecting)
            //8/ Set cooldown
            if (ability.Cooldown > 0)
                source.SetCooldown(ability);
            return true;
        }

        #endregion

        // TEST: TO REMOVE
        public bool Process(ICharacter source, ICharacter target, IAbility ability)
        {
            ProcessOnOneTarget(source, target, ability);
            return true;
        }

        private static void ProcessOnOneTarget(ICharacter source, ICharacter victim, IAbility ability)
        {
            if (ability?.Effects == null || ability.Effects.Count == 0 || !source.IsValid || !victim.IsValid)
                return;
            foreach (AbilityEffect effect in ability.Effects)
                effect.Process(source, victim, ability);
        }

        public IAbility Search(IEnumerable<AbilityAndLevel> abilities, int level, CommandParameter parameter)
        {
            return abilities.Where(x =>
                (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed
                && (x.Ability.Flags & AbilityFlags.Passive) != AbilityFlags.Passive
                && x.Level <= level
                && FindHelpers.StringStartWith(x.Ability.Name, parameter.Value))
                .Select(x => x.Ability)
                .ElementAtOrDefault(parameter.Count - 1);
        }

    }
}
