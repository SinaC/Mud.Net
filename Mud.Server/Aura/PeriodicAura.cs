using System;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;

namespace Mud.Server.Aura
{
    public class PeriodicAura : IPeriodicAura
    {
        private DateTime _startTime;
        private DateTime _lastTickElapsed;

        protected ITimeHandler TimeHandler => DependencyContainer.Current.GetInstance<ITimeHandler>();

        #region IPeriodicAura

        public IAbility Ability { get; }
        public PeriodicAuraTypes AuraType { get; }
        public IEntity Source { get; private set; }
        public SchoolTypes School { get; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; }
        public int Level { get; }
        public bool TickVisible { get; }
        public int TotalTicks { get; }
        public int TickDelay { get; private set; }
        public int TicksLeft { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = TimeHandler.CurrentTime - _startTime;
                return TotalTicks*TickDelay - (int) Math.Ceiling(ts.TotalSeconds);
            }
        }

        #endregion

        public PeriodicAura(IAbility ability, PeriodicAuraTypes auraType, IEntity source, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
        {
            _startTime = TimeHandler.CurrentTime;
            _lastTickElapsed = TimeHandler.CurrentTime;

            Ability = ability;
            AuraType = auraType;
            Source = source;
            Amount = amount;
            AmountOperator = amountOperator;
            Level = level;
            TickVisible = tickVisible;
            TickDelay = tickDelay;
            TicksLeft = totalTicks;
            TotalTicks = totalTicks;
        }

        public PeriodicAura(IAbility ability, PeriodicAuraTypes auraType, IEntity source, SchoolTypes school, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
            : this(ability, auraType, source, amount, amountOperator, level, tickVisible, tickDelay, totalTicks)
        {
            School = school;
        }

        // Reset source
        public void ResetSource()
        {
            Source = null;
        }

        // Process periodic aura (return true if aura is elapsed)
        public bool Process(ICharacter victim)
        {
            if (TicksLeft <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PeriodicAura.Process: no tick left");
                return true;
            }

            // Set/Reset start time on first tick
            DateTime now = TimeHandler.CurrentTime;
            if (TicksLeft == TotalTicks) // first tick
                _startTime = now;

            // Compute delay between now and previous check
            TimeSpan diff = now - _lastTickElapsed;
            // If TickDelay in seconds elapsed, perform heal/damage
            if (diff.TotalSeconds > TickDelay)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Processing periodic aura {0} on {1} from {2} tick left {3} delay {4}", Ability == null ? "<<??>>" : Ability.Name, victim.Name, Source == null ? "<<??>>" : Source.Name, TicksLeft, TickDelay);

                _lastTickElapsed = now;
                TicksLeft--;

                int amount = Amount;
                if (AmountOperator == AmountOperators.Percentage)
                    amount = victim[SecondaryAttributeTypes.MaxHitPoints] * Amount / 100; // percentage of max hit points
                if (AuraType == PeriodicAuraTypes.Heal)
                    victim.Heal(Source, Ability, amount, TickVisible);
                else if (AuraType == PeriodicAuraTypes.Damage)
                {
                    if (Source == null)
                        victim.UnknownSourceDamage(Ability, amount, School, TickVisible);
                    else if (Source is ICharacter character)
                        victim.AbilityDamage(character, Ability, amount, School, TickVisible);
                    else
                        victim.UnknownSourceDamage(Ability, amount, School, TickVisible);
                }
            }
            return TicksLeft == 0;
        }

        // Refresh with a new aura
        public void Refresh(IPeriodicAura aura)
        {
            _startTime = TimeHandler.CurrentTime;
            // Refresh aura values
            TicksLeft = aura.TotalTicks;
            TickDelay = aura.TickDelay;
            Amount = aura.Amount;
        }

        // Called when dispelled
        public virtual void OnDispelled(IEntity dispelSource)
        {
        }

        // Called when vanished
        public virtual void OnVanished()
        {
        }
    }
}
