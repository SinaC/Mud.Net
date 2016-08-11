using System;
using System.Linq;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Helpers;

namespace Mud.Server.Abilities
{
    public abstract class AbilityEffect
    {
        public abstract bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult);

        protected int ComputeAttributeBasedAmount(ICharacter source, SecondaryAttributeTypes attribute, float factor)
        {
            return (int)Math.Ceiling(factor * source[attribute] / 100.0f);
        }

        protected void PerformDamage(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult, int damage, SchoolTypes damageType)
        {
            if (ability.Kind == AbilityKinds.Skill)
            {
                // TODO: refactor same code in Character.OneHit
                switch (attackResult)
                {
                    case CombatHelpers.AttackResults.Miss:
                    case CombatHelpers.AttackResults.Dodge:
                    case CombatHelpers.AttackResults.Parry:
                        return; // no damage
                    case CombatHelpers.AttackResults.Block:
                        damage = (damage * 7) / 10;
                        break;
                    case CombatHelpers.AttackResults.Critical:
                        // TODO http://wow.gamepedia.com/Critical_strike
                        if (victim.ImpersonatedBy != null)
                            damage *= 2;
                        else
                            damage = (damage * 150) / 200;
                        break;
                    case CombatHelpers.AttackResults.CrushingBlow:
                        // http://wow.gamepedia.com/Crushing_Blow
                        damage = (damage * 150) / 200;
                        break;
                    case CombatHelpers.AttackResults.Hit:
                        // no modified
                        break;
                }
            }
            else if (ability.Kind == AbilityKinds.Spell)
            {
                // Miss/Hit/Critical
                switch (attackResult)
                {
                    case CombatHelpers.AttackResults.Miss:
                        return;  // no damage
                    case CombatHelpers.AttackResults.Critical:
                        // http://wow.gamepedia.com/Spell_critical_strike
                        damage *= 2;
                        break;
                    case CombatHelpers.AttackResults.Hit:
                        // no modifier
                        break;
                }
            }
            victim.AbilityDamage(source, ability, damage, damageType, true);
        }
    }

    public class DamageAbilityEffect : AbilityEffect
    {
        public float Factor { get; }
        public SecondaryAttributeTypes Attribute { get; }
        public SchoolTypes School { get; }

        public DamageAbilityEffect(float factor, SecondaryAttributeTypes attribute, SchoolTypes school)
        {
            Factor = factor;
            Attribute = attribute;
            School = school;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = ComputeAttributeBasedAmount(source, Attribute, Factor);
            PerformDamage(source, victim, ability, attackResult, amount, School);
            return true;
        }
    }

    public class DamageRangeAbilityEffect : AbilityEffect
    {
        public int MinDamage { get; }
        public int MaxDamage { get; }
        public SchoolTypes School { get; }

        public DamageRangeAbilityEffect(int minDamage, int maxDamage, SchoolTypes school)
        {
            MinDamage = minDamage;
            MaxDamage = maxDamage;
            School = school;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = RandomizeHelpers.Instance.Randomizer.Next(MinDamage, MaxDamage + 1);
            PerformDamage(source, victim, ability, attackResult, amount, School);
            return true;
        }
    }

    public class HealAbilityEffect : AbilityEffect
    {
        public float Factor { get; }
        public SecondaryAttributeTypes Attribute { get; }

        public HealAbilityEffect(float factor, SecondaryAttributeTypes attribute)
        {
            Factor = factor;
            Attribute = attribute;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = ComputeAttributeBasedAmount(source, Attribute, Factor);
            victim.Heal(source, ability, amount, true);
            return true;
        }
    }

    public class HealSourceAbilityEffect : AbilityEffect
    {
        public float Factor { get; }
        public SecondaryAttributeTypes Attribute { get; }

        public HealSourceAbilityEffect(float factor, SecondaryAttributeTypes attribute)
        {
            Factor = factor;
            Attribute = attribute;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = ComputeAttributeBasedAmount(source, Attribute, Factor);
            source.Heal(source, ability, amount, true);
            return true;
        }
    }

    public class AuraAbilityEffect : AbilityEffect
    {
        public AuraModifiers Modifier { get; }
        public int Amount { get; }
        public AmountOperators AmountOperator { get; }

        public AuraAbilityEffect(AuraModifiers modifier, int amount, AmountOperators op)
        {
            Modifier = modifier;
            Amount = amount;
            AmountOperator = op;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            Repository.World.AddAura(victim, ability, source, Modifier, Amount, AmountOperator, ability.Duration, true);
            return true;
        }
    }

    public class DotAbilityEffect : AbilityEffect
    {
        public float Factor { get; }
        public SecondaryAttributeTypes Attribute { get; }
        public SchoolTypes School { get; }
        public int TickDelay { get; }

        public DotAbilityEffect(float factor, SecondaryAttributeTypes attribute, SchoolTypes school, int tickDelay)
        {
            Factor = factor;
            Attribute = attribute;
            School = school;
            TickDelay = tickDelay;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = ComputeAttributeBasedAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            Repository.World.AddPeriodicAura(victim, ability, source, School, amount, AmountOperators.Fixed, true, TickDelay, totalTicks);
            return true;
        }
    }

    public class HotAbilityEffect : AbilityEffect
    {
        public float Factor { get; }
        public SecondaryAttributeTypes Attribute { get; }
        public int TickDelay { get; }

        public HotAbilityEffect(float factor, SecondaryAttributeTypes attribute, int tickDelay)
        {
            Factor = factor;
            Attribute = attribute;
            TickDelay = tickDelay;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            int amount = ComputeAttributeBasedAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            Repository.World.AddPeriodicAura(victim, ability, source, amount, AmountOperators.Fixed, true, TickDelay, totalTicks);
            return true;
        }
    }

    public class DamageOrHealEffect : AbilityEffect
    {
        public float DamageFactor { get; }
        public float HealFactor { get; }
        public SecondaryAttributeTypes Attribute { get; }
        public SchoolTypes School { get; }

        public DamageOrHealEffect(float damageFactor, float healFactor, SecondaryAttributeTypes attribute, SchoolTypes school)
        {
            DamageFactor = damageFactor;
            HealFactor = healFactor;
            Attribute = attribute;
            School = school;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            if (source == victim)
            {
                int amount = ComputeAttributeBasedAmount(source, Attribute, HealFactor);
                victim.Heal(source, ability, amount, true);
            }
            else
            {
                int amount = ComputeAttributeBasedAmount(source, Attribute, DamageFactor);
                PerformDamage(source, victim, ability, attackResult, amount, School);
            }
            return true;
        }
    }

    public class DispelEffect : AbilityEffect
    {
        public DispelTypes DispelType { get; }
        public bool Offensive { get; private set; } // if true, remove a buff, else remove a debuff

        public DispelEffect(DispelTypes dispelType, bool offensive)
        {
            DispelType = dispelType;
            Offensive = offensive;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            // TODO: difference between buff/debuff to handle Offensive flag

            // Check periodic aura
            IPeriodicAura periodicAura = victim.PeriodicAuras.FirstOrDefault(x => x.Ability != null && x.Ability.DispelType == DispelType);
            if (periodicAura != null)
            {
                victim.RemovePeriodicAura(periodicAura);
                return true;
            }

            // Check aura
            IAura aura = victim.Auras.FirstOrDefault(x => x.Ability != null && x.Ability.DispelType == DispelType);
            if (aura != null)
            {
                victim.RemoveAura(aura, true);
                return true;
            }
            return true;
        }
    }

    public class PowerWordShieldEffect : AbilityEffect
    {
        public override bool Process(ICharacter source, ICharacter victim, IAbility ability, CombatHelpers.AttackResults attackResult)
        {
            if (victim.Auras.Any(x => x.Ability != null && x.Ability == Repository.AbilityManager.WeakenedSoulAbility))
            {
                source.Act(ActOptions.ToCharacter, "{0} cannot be targetted by {1}.", victim, ability.Name);
                return false;
            }
            int amount = ComputeAttributeBasedAmount(source, SecondaryAttributeTypes.SpellPower, 45.9f);
            Repository.World.AddAura(victim, ability, source, AuraModifiers.DamageAbsorb, amount, AmountOperators.Fixed, ability.Duration, true);
            Repository.World.AddAura(victim, Repository.AbilityManager.WeakenedSoulAbility, source, AuraModifiers.None, 0, AmountOperators.None, 15, true);
            return true;
        }
    }
}
