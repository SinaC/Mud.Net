using System;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class PeriodicEffect : IPeriodicEffect
    {
        private DateTime _lastPeriodElapsed;

        public string Name { get; private set; }
        public EffectTypes EffectType { get; private set; }
        public ICharacter Source { get; private set; }
        public DamageTypes DamageType { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }
        public bool Visible { get; private set; }
        public int PeriodInSeconds { get; private set; }
        public int PeriodsLeft { get; private set; }

        public PeriodicEffect(string name, EffectTypes effectType, ICharacter source, int amount, AmountOperators amountOperator, bool visible, int periodInSeconds, int periodsLeft)
        {
            _lastPeriodElapsed = DateTime.Now;

            Name = name;
            EffectType = effectType;
            Source = source;
            Amount = amount;
            AmountOperator = amountOperator;
            Visible = visible;
            PeriodInSeconds = periodInSeconds;
            PeriodsLeft = periodsLeft;
        }

        public PeriodicEffect(string name, EffectTypes effectType, ICharacter source, DamageTypes damageType, int amount, AmountOperators amountOperator, bool visible, int periodInSeconds, int periodsLeft)
            : this(name, effectType, source, amount, amountOperator, visible, periodInSeconds, periodsLeft)
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
            if (PeriodsLeft <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PeriodicEffect.Process: no period left");
                return true;
            }

            // Compute delay between now and previous check
            DateTime now = DateTime.Now;
            TimeSpan diff = now - _lastPeriodElapsed;
            // If periodInSeconds in seconds elapsed, perform heal/damage
            if (diff.TotalSeconds > PeriodInSeconds)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Processing periodic effect {0} on {1} period {2} left {3}", Name, victim.Name, PeriodsLeft, PeriodInSeconds);

                _lastPeriodElapsed = now;
                PeriodsLeft--;

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
            return PeriodsLeft == 0;
        }
    }
}
