using System;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Effects
{
    public class PeriodicEffect : IPeriodicEffect
    {
        private readonly DateTime _startTime;
        private DateTime _lastTickElapsed;

        public string Name { get; private set; }
        public EffectTypes EffectType { get; private set; }
        public ICharacter Source { get; private set; }
        public DamageTypes DamageType { get; private set; }
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
            _startTime = DateTime.Now;
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

        public PeriodicEffect(string name, EffectTypes effectType, ICharacter source, DamageTypes damageType, int amount, AmountOperators amountOperator, bool visible, int tickDelay, int ticksLeft)
            : this(name, effectType, source, amount, amountOperator, visible, tickDelay, ticksLeft)
        {
            DamageType = damageType;
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

            // Compute delay between now and previous check
            DateTime now = DateTime.Now;
            TimeSpan diff = now - _lastTickElapsed;
            // If TickDelay in seconds elapsed, perform heal/damage
            if (diff.TotalSeconds > TickDelay)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Processing periodic effect {0} on {1} tick left {2} delay {3}", Name, victim.Name, TicksLeft, TickDelay);

                _lastTickElapsed = now;
                TicksLeft--;

                int amount = Amount;
                if (AmountOperator == AmountOperators.Percentage)
                    amount = victim.HitPoints*Amount/100;
                if (EffectType == EffectTypes.Heal)
                    victim.Heal(Source, Name, amount, Visible);
                else if (EffectType == EffectTypes.Damage)
                {
                    if (Source == null)
                        victim.UnknownSourceDamage(Name, amount, DamageType, Visible);
                    else
                        victim.CombatDamage(Source, Name, amount, DamageType, Visible);
                }
            }
            return TicksLeft == 0;
        }
    }
}
