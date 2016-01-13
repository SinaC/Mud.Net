using System;
using System.Linq;
using Mud.Server.Constants;
using Mud.Server.World;

namespace Mud.Server.Abilities
{
    public abstract class AbilityEffect
    {
        public int ComputeAmount(ICharacter source, ComputedAttributeTypes attribute, float factor)
        {
            return (int)Math.Ceiling(factor * source[attribute] / 100.0f);
        }

        public abstract bool Process(ICharacter source, ICharacter victim, IAbility ability);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            victim.Damage(source, ability, amount, School, true);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            victim.Heal(source, ability, amount, true);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            World.World.Instance.AddAura(victim, ability, Modifier, Amount, AmountOperator, ability.Duration, true);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            World.World.Instance.AddPeriodicAura(victim, ability, source, School, amount, AmountOperators.Fixed, true, TickDelay, totalTicks);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            int amount = ComputeAmount(source, Attribute, Factor);
            int totalTicks = ability.Duration / TickDelay;
            World.World.Instance.AddPeriodicAura(victim, ability, source, amount, AmountOperators.Fixed, true, TickDelay, totalTicks);
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

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            if (source == victim)
            {
                int amount = ComputeAmount(source, Attribute, HealFactor);
                victim.Heal(source, ability, amount, true);
            }
            else
            {
                int amount = ComputeAmount(source, Attribute, DamageFactor);
                victim.Damage(source, ability, amount, School, true);
            }
            return true;
        }
    }

    public class DispelEffect : AbilityEffect
    {
        public DispelTypes DispelType { get; private set; }
        public bool Offensive { get; private set; } // if true, remove a buff, else remove a debuff

        public DispelEffect(DispelTypes dispelType, bool offensive)
        {
            DispelType = dispelType;
            Offensive = offensive;
        }

        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
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
        public override bool Process(ICharacter source, ICharacter victim, IAbility ability)
        {
            if (victim.Auras.Any(x => x.Ability == AbilityManager.WeakenedSoulAbility))
            {
                source.Act(ActOptions.ToCharacter, "{0} cannot be targetted by {1}.", victim, ability.Name);
                return false;
            }
            int amount = ComputeAmount(source, ComputedAttributeTypes.SpellPower, 45.9f);
            World.World.Instance.AddAura(victim, ability, AuraModifiers.Shield, amount, AmountOperators.Fixed, ability.Duration, true);
            World.World.Instance.AddAura(victim, AbilityManager.WeakenedSoulAbility, AuraModifiers.None, 0, AmountOperators.None, 15, true);
            return true;
        }
    }
}
