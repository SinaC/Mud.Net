using System;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Aura
{
    // TODO: linked to an ability
    public class PeriodicAura : IPeriodicAura
    {
        private DateTime _startTime;
        private DateTime _lastTickElapsed;

        public string Name { get; private set; }
        public PeriodicAuraTypes AuraType { get; private set; }
        public ICharacter Source { get; private set; }
        public SchoolTypes School { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }
        public bool TickVisible { get; private set; }
        public int TotalTicks { get; private set; }
        public int TickDelay { get; private set; }
        public int TicksLeft { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = Server.Server.Instance.CurrentTime - _startTime;
                return TotalTicks*TickDelay - (int) Math.Ceiling(ts.TotalSeconds);
            }
        }

        public PeriodicAura(string name, PeriodicAuraTypes auraType, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            _lastTickElapsed = Server.Server.Instance.CurrentTime;

            Name = name;
            AuraType = auraType;
            Source = source;
            Amount = amount;
            AmountOperator = amountOperator;
            TickVisible = tickVisible;
            TickDelay = tickDelay;
            TicksLeft = totalTicks;
            TotalTicks = totalTicks;
        }

        public PeriodicAura(string name, PeriodicAuraTypes auraType, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
            : this(name, auraType, source, amount, amountOperator, tickVisible, tickDelay, totalTicks)
        {
            School = school;
        }

        // Reset source
        public void ResetSource()
        {
            Source = null;
        }

        public bool Process(ICharacter victim)
        {
            if (TicksLeft <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PeriodicAura.Process: no tick left");
                return true;
            }

            // Set start time on first tick
            DateTime now = Server.Server.Instance.CurrentTime;
            if (TicksLeft == TotalTicks) // first tick
                _startTime = now;

            // Compute delay between now and previous check
            TimeSpan diff = now - _lastTickElapsed;
            // If TickDelay in seconds elapsed, perform heal/damage
            if (diff.TotalSeconds > TickDelay)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Processing periodic aura {0} on {1} from {2} tick left {3} delay {4}", Name, victim.Name, Source == null ? "<<??>>" : Source.Name, TicksLeft, TickDelay);

                _lastTickElapsed = now;
                TicksLeft--;

                int amount = Amount;
                if (AmountOperator == AmountOperators.Percentage)
                    amount = victim.GetComputedAttribute(ComputedAttributeTypes.MaxHitPoints) * Amount / 100; // percentage of max hit points
                if (AuraType == PeriodicAuraTypes.Heal)
                    victim.Heal(Source, Name, amount, TickVisible);
                else if (AuraType == PeriodicAuraTypes.Damage)
                {
                    if (Source == null)
                        victim.UnknownSourceDamage(Name, amount, School, TickVisible);
                    else
                        victim.CombatDamage(Source, Name, amount, School, TickVisible);
                }
            }
            return TicksLeft == 0;
        }
    }
}
