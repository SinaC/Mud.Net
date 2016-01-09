using Mud.Server.Auras;
using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    // By default: no cost, GCD 1, no CD, no duration, no school/mechanic/dispel type, Cannot be used when shapeshifted
    public abstract class AbilityBase : IAbility
    {
        // Name
        public abstract string Name { get; }

        // Cost
        public abstract ResourceKinds ResourceKind { get; }

        public abstract AmountOperators CostType { get; }

        public abstract int CostAmount { get; }

        // GCD/CD/Duration
        public virtual int GlobalCooldown
        {
            get { return 1; }
        }

        public virtual int Cooldown
        {
            get { return 0; }
        }

        public abstract int Duration { get; }

        // School/Mechanic/DispelType
        public abstract SchoolTypes School { get; }

        public virtual AbilityMechanics Mechanic
        {
            get { return AbilityMechanics.None; }
        }

        public virtual DispelTypes DispelType
        {
            get { return DispelTypes.None; }
        }

        // Flags
        public virtual AbilityFlags Flags
        {
            get { return AbilityFlags.CannotBeUsedWhileShapeshifted; }
        }

        // Process
        public abstract bool Process(ICharacter source, params CommandParameter[] parameters);

        //
        protected void AddDot(ICharacter source, ICharacter victim, int amount, int tickDelay, AmountOperators op = AmountOperators.Fixed)
        {
            victim.AddPeriodicAura(new PeriodicAura(Name, PeriodicAuraTypes.Damage, source, School, amount, op, true, tickDelay, Duration / tickDelay));
        }

        protected void AddHot(ICharacter source, ICharacter victim, int amount, int tickDelay, AmountOperators op = AmountOperators.Fixed)
        {
            victim.AddPeriodicAura(new PeriodicAura(Name, PeriodicAuraTypes.Heal, source, School, amount, op, true, tickDelay, Duration / tickDelay));
        }

        protected void Damage(ICharacter source, ICharacter victim, int amount)
        {
            victim.CombatDamage(source, Name, amount, School, true);
        }

        protected void Heal(ICharacter source, ICharacter victim, int amount)
        {
            victim.Heal(source, Name, amount, true);
        }
    }
}
