using System;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Aura
{
    public class PeriodicAura : IPeriodicAura
    {
        private DateTime _startTime;
        private DateTime _lastTickElapsed;

        #region IPeriodicAura

        public IAbility Ability { get; }
        public PeriodicAuraTypes AuraType { get; }
        public ICharacter Source { get; private set; }
        public SchoolTypes School { get; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; }
        public bool TickVisible { get; }
        public int TotalTicks { get; }
        public int TickDelay { get; private set; }
        public int TicksLeft { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = DependencyContainer.Instance.GetInstance<IServer>().CurrentTime - _startTime;
                return TotalTicks*TickDelay - (int) Math.Ceiling(ts.TotalSeconds);
            }
        }

        #endregion

        public PeriodicAura(IAbility ability, PeriodicAuraTypes auraType, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            _startTime = DependencyContainer.Instance.GetInstance<IServer>().CurrentTime;
            _lastTickElapsed = DependencyContainer.Instance.GetInstance<IServer>().CurrentTime;

            Ability = ability;
            AuraType = auraType;
            Source = source;
            Amount = amount;
            AmountOperator = amountOperator;
            TickVisible = tickVisible;
            TickDelay = tickDelay;
            TicksLeft = totalTicks;
            TotalTicks = totalTicks;
        }

        public PeriodicAura(IAbility ability, PeriodicAuraTypes auraType, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
            : this(ability, auraType, source, amount, amountOperator, tickVisible, tickDelay, totalTicks)
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
            DateTime now = DependencyContainer.Instance.GetInstance<IServer>().CurrentTime;
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
                    else
                        victim.AbilityDamage(Source, Ability, amount, School, TickVisible);
                }
            }
            return TicksLeft == 0;
        }

        // Refresh with a new aura
        public void Refresh(IPeriodicAura aura)
        {
            _startTime = DependencyContainer.Instance.GetInstance<IServer>().CurrentTime;
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
