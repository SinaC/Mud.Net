using System;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Effects
{
    // TODO: time computation are totally wrong (it's possible to get -2 seconds left)
    public class PeriodicEffect : IPeriodicEffect
    {
        private DateTime _startTime;
        private DateTime _lastTickElapsed;

        public string Name { get; private set; }
        public EffectTypes EffectType { get; private set; }
        public ICharacter Source { get; private set; }
        public SchoolTypes SchoolType { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }
        public bool Visible { get; private set; }
        public int TotalTicks { get; private set; }
        public int TickDelay { get; private set; }
        public int TicksLeft { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = DateTime.Now - _startTime;
                return TotalTicks*TickDelay - (int) Math.Ceiling(ts.TotalSeconds);
            }
        }

        public PeriodicEffect(string name, EffectTypes effectType, ICharacter source, int amount, AmountOperators amountOperator, bool visible, int tickDelay, int ticksLeft)
        {
            _lastTickElapsed = DateTime.Now;

            Name = name;
            EffectType = effectType;
            Source = source;
            Amount = amount;
            AmountOperator = amountOperator;
            Visible = visible;
            TickDelay = tickDelay;
            TicksLeft = ticksLeft;
            TotalTicks = ticksLeft;
        }

        public PeriodicEffect(string name, EffectTypes effectType, ICharacter source, SchoolTypes schoolType, int amount, AmountOperators amountOperator, bool visible, int tickDelay, int ticksLeft)
            : this(name, effectType, source, amount, amountOperator, visible, tickDelay, ticksLeft)
        {
            SchoolType = schoolType;
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
                Log.Default.WriteLine(LogLevels.Error, "PeriodicEffect.Process: no period left");
                return true;
            }

            // Set start time on first tick
            DateTime now = DateTime.Now;
            if (TicksLeft == TotalTicks) // first tick
                _startTime = now;

            // Compute delay between now and previous check
            TimeSpan diff = now - _lastTickElapsed;
            // If TickDelay in seconds elapsed, perform heal/damage
            if (diff.TotalSeconds > TickDelay)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Processing periodic effect {0} on {1} from {2} tick left {3} delay {4}", Name, victim.Name, Source == null ? "<<??>>" : Source.Name, TicksLeft, TickDelay);

                _lastTickElapsed = now;
                TicksLeft--;

                int amount = Amount;
                if (AmountOperator == AmountOperators.Percentage)
                    amount = victim.MaxHitPoints * Amount / 100; // percentage of max hit points
                if (EffectType == EffectTypes.Heal)
                    victim.Heal(Source, Name, amount, Visible);
                else if (EffectType == EffectTypes.Damage)
                {
                    if (Source == null)
                        victim.UnknownSourceDamage(Name, amount, SchoolType, Visible);
                    else
                        victim.CombatDamage(Source, Name, amount, SchoolType, Visible);
                }
            }
            return TicksLeft == 0;
        }
    }
}
