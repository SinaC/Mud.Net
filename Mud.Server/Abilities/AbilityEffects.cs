using System;
using System.Linq;
using Mud.Server.Auras;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public abstract class AbilityEffect
    {
        public int ComputeAmount(ICharacter source, ComputedAttributeTypes attribute, float factor)
        {
            return (int)Math.Ceiling(factor * source.GetComputedAttribute(attribute) / 100.0f);
        }

        public abstract bool Process(ICharacter source, ICharacter victim, Ability ability);
    }

    public class DamageAbilityEffect : AbilityEffect
    {
        public float Factor { get; private set; }
        public ComputedAttributeTypes Attribute { get; private set; }
        public SchoolTypes School { get; private set; }

        public DamageAbilityEffect(float factor, ComputedAttributeTypes attribute, SchoolTypes school)
        {
            Factor = factor;
            Attribute = attribute;
            School = school;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            victim.CombatDamage(source, ability.Name, amount, School, true);
            return true;
        }
    }

    public class HealAbilityEffect : AbilityEffect
    {
        public float Factor { get; private set; }
        public ComputedAttributeTypes Attribute { get; private set; }

        public HealAbilityEffect(float factor, ComputedAttributeTypes attribute)
        {
            Factor = factor;
            Attribute = attribute;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            victim.Heal(source, ability.Name, amount, true);
            return true;
        }
    }

    public class AuraAbilityEffect : AbilityEffect
    {
        public AuraModifiers Modifier { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }

        public AuraAbilityEffect(AuraModifiers modifier, int amount, AmountOperators op)
        {
            Modifier = modifier;
            Amount = amount;
            AmountOperator = op;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            victim.AddAura(new Aura(ability.Name, Modifier, Amount, AmountOperator, ability.Duration), true);
            return true;
        }
    }

    public class DotAbilityEffect : AbilityEffect
    {
        public float Factor { get; private set; }
        public ComputedAttributeTypes Attribute { get; private set; }
        public SchoolTypes School { get; private set; }
        public int TickDelay { get; private set; }

        public DotAbilityEffect(float factor, ComputedAttributeTypes attribute, SchoolTypes school, int tickDelay)
        {
            Factor = factor;
            Attribute = attribute;
            School = school;
            TickDelay = tickDelay;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            victim.AddPeriodicAura(new PeriodicAura(ability.Name, PeriodicAuraTypes.Damage, source, School, amount, AmountOperators.Fixed, true, TickDelay, totalTicks));
            return true;
        }
    }

    public class HotAbilityEffect : AbilityEffect
    {
        public float Factor { get; private set; }
        public ComputedAttributeTypes Attribute { get; private set; }
        public int TickDelay { get; private set; }

        public HotAbilityEffect(float factor, ComputedAttributeTypes attribute, int tickDelay)
        {
            Factor = factor;
            Attribute = attribute;
            TickDelay = tickDelay;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            victim.AddPeriodicAura(new PeriodicAura(ability.Name, PeriodicAuraTypes.Heal, source, amount, AmountOperators.Fixed, true, TickDelay, totalTicks));
            return true;
        }
    }

    public class DamageOrHealEffect : AbilityEffect
    {
        public float DamageFactor { get; private set; }
        public float HealFactor { get; private set; }
        public ComputedAttributeTypes Attribute { get; private set; }
        public SchoolTypes School { get; private set; }

        public DamageOrHealEffect(float damageFactor, float healFactor, ComputedAttributeTypes attribute, SchoolTypes school)
        {
            DamageFactor = damageFactor;
            HealFactor = healFactor;
            Attribute = attribute;
            School = school;
        }

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            if (source == victim)
            {
                int amount = ComputeAmount(source, Attribute, HealFactor);
                victim.Heal(source, ability.Name, amount, true);
            }
            else
            {
                int amount = ComputeAmount(source, Attribute, DamageFactor);
                victim.CombatDamage(source, ability.Name, amount, School, true);
            }
            return true;
        }
    }

    public class PowerWordShieldEffect : AbilityEffect
    {
        public const string WeakenedSoulDebuff = "Weakened Soul";

        public override bool Process(ICharacter source, ICharacter victim, Ability ability)
        {
            if (victim.Auras.Any(x => x.Name == WeakenedSoulDebuff))
            {
                source.Act(ActOptions.ToCharacter, "{0} cannot be targetted by {1}.", victim, ability.Name);
                return false;
            }
            victim.AddAura(new Aura(ability.Name, AuraModifiers.Shield, 459 * source.GetComputedAttribute(ComputedAttributeTypes.SpellPower) / 1000, AmountOperators.Fixed, ability.Duration), true);
            victim.AddAura(new Aura(WeakenedSoulDebuff, AuraModifiers.None, 0, AmountOperators.None, 15), true);
            return true;
        }
    }
}
