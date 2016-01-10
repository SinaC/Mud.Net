using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    public class AbilityManager
    {
        public List<Ability> Abilities = new List<Ability>
        {
            new Ability(1, "Wrath", AbilityTargets.Target, ResourceKinds.Mana, AmountOperators.Percentage, 4, 1, 0, 0, SchoolTypes.Nature, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageAbilityEffect(149, ComputedAttributeTypes.SpellPower, SchoolTypes.Nature)),
            new Ability(2, "Trash", AbilityTargets.Room, ResourceKinds.Energy, AmountOperators.Fixed, 50, 1, 0, 15, SchoolTypes.Physical, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.None, new DamageAbilityEffect(513, ComputedAttributeTypes.AttackPower, SchoolTypes.Physical), new DotAbilityEffect(365, ComputedAttributeTypes.SpellPower, SchoolTypes.Physical, 3)),
            new Ability(3, "Shadow Word: Pain", AbilityTargets.Target, ResourceKinds.Mana, AmountOperators.Percentage, 1, 1, 0, 18, SchoolTypes.Shadow, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new DamageAbilityEffect(475, ComputedAttributeTypes.SpellPower, SchoolTypes.Shadow), new DotAbilityEffect(475, ComputedAttributeTypes.SpellPower, SchoolTypes.Shadow, 3)),
            new Ability(4, "Rupture", AbilityTargets.Target, ResourceKinds.Energy, AmountOperators.Fixed, 25, 1, 0, 8/* TODO: multiplied by combo*/, SchoolTypes.Physical, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.None, new DotAbilityEffect(685, ComputedAttributeTypes.AttackPower, SchoolTypes.Physical, 2)),
            new Ability(5, "Renew", AbilityTargets.TargetOrSelf, ResourceKinds.Mana, AmountOperators.Percentage, 2, 1, 0, 12, SchoolTypes.Holy, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new HealAbilityEffect(22, ComputedAttributeTypes.SpellPower), new HotAbilityEffect(44, ComputedAttributeTypes.SpellPower, 3)),
            new Ability(6, "Power Word: Shield", AbilityTargets.TargetOrSelf, ResourceKinds.Mana, AmountOperators.Percentage, 2, 1, 6, 15, SchoolTypes.Holy, AbilityMechanics.Shielded, DispelTypes.Magic, AbilityFlags.None, new PowerWordShieldEffect()),
            new Ability(7, "Death Coil", AbilityTargets.TargetOrSelf, ResourceKinds.Runic, AmountOperators.Fixed, 30, 1, 0, 30, SchoolTypes.Shadow, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageOrHealEffect(0.88f, 0.88f*5, ComputedAttributeTypes.AttackPower, SchoolTypes.Shadow), new AuraAbilityEffect(AuraModifiers.MaxHitPoints, 3, AmountOperators.Percentage)),
            new Ability(8, "Berserking", AbilityTargets.Self, ResourceKinds.None, AmountOperators.None, 0, 1, 3*60, 10, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackSpeed, 15, AmountOperators.Percentage)),
            new Ability(9, "Battle Shout", AbilityTargets.Group, ResourceKinds.None, AmountOperators.None, 0, 1, 0, 1*60*60, SchoolTypes.Physical, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackPower, 10, AmountOperators.Percentage))
        };

        public bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            //0/ Search ability
            if (parameters.Length == 0)
            {
                source.Send("Cast/use what ?" + Environment.NewLine);
                return false;
            }
            Ability ability = Abilities.FirstOrDefault(x => x.Name.StartsWith(parameters[0].Value, StringComparison.InvariantCultureIgnoreCase));
            if (ability == null)
            {
                source.Send("Unknown ability." + Environment.NewLine);
                return false;
            }
            // TODO: //1/ check cooldown
            // TODO: //2/ check resource
            //3/ Check target(s)
            IEnumerable<ICharacter> targets;
            switch (ability.Target)
            {
                case AbilityTargets.Self:
                    targets = new[] { source };
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
                        targets = new[] { target };
                        break;
                    }
                case AbilityTargets.TargetOrSelf:
                    {
                        if (parameters.Length == 1)
                            targets = new[] { source };
                        else
                        {
                            ICharacter target = FindHelpers.FindByName(source.Room.People, parameters[1]);
                            if (target == null)
                            {
                                source.Send(StringHelpers.CharacterNotFound);
                                return false;
                            }
                            targets = new[] { target };
                        }
                        break;
                    }
                case AbilityTargets.Group:
                    // TODO: everyone in groups
                    targets = new[] { source };
                    break;
                case AbilityTargets.Room:
                    targets = source.Room.People.Where(x => x != source); // TODO: only enemy
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown target {0} for ability {1}[{2}]!!!", ability.Target, ability.Name, ability.Id);
                    targets = Enumerable.Empty<ICharacter>();
                    break;
            }
            //4/ Say ability
            source.Send("You use '{0}'."+Environment.NewLine, ability.Name);
            // TODO //5/ Pay resource cost
            //6/ Perform effect(s) on target(s)
            foreach (ICharacter target in targets)
            {
                if (source != target)
                    target.Act(ActOptions.ToCharacter, "{0} uses '{1}' on you.", source, ability.Name);
                ProcessOnOneTarget(source, target, ability);
            }
            //7/ Set global cooldown
            if (source.ImpersonatedBy != null)
                source.ImpersonatedBy.SetGlobalCooldown(ability.GlobalCooldown);
            // TODO //8/ Set cooldown
            return true;
        }

        // TEST: TO REMOVE
        public bool Process(ICharacter source, ICharacter target, Ability ability)
        {
            ProcessOnOneTarget(source, target, ability);
            return true;
        }

        private void ProcessOnOneTarget(ICharacter source, ICharacter victim, Ability ability)
        {
            if (ability == null || ability.Effects == null || ability.Effects.Count == 0
                || !source.IsValid || !victim.IsValid)
                return;
            foreach (AbilityEffect effect in ability.Effects)
                effect.Process(source, victim, ability);
        }
    }
}
