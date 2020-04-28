using System;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Common;

namespace Mud.Server.Aura
{
    // TODO: Aura with multiple modifiers
    // TODO: permanent aura: TotalSeconds = -1
    // TODO: change TotalSeconds to PulseLeft
    public class Aura : IAura
    {
        protected ITimeHandler TimeHandler => DependencyContainer.Current.GetInstance<ITimeHandler>();

        #region IAura

        public IAbility Ability { get; private set; }
        public IEntity Source { get; private set; }
        public AuraModifiers Modifier { get; }
        public int Amount { get; private set; } // can be a flag
        public AmountOperators AmountOperator { get; } // Amount is a flag is AmountOperator is Flags
        public int Level { get; private set; }
        public DateTime StartTime { get; private set; }
        public int PulseLeft { get; private set; }

        #endregion

        public Aura(IAbility ability, IEntity source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int level, TimeSpan ts)
        {
            StartTime = TimeHandler.CurrentTime;

            Ability = ability;
            Source = source;
            Modifier = modifier;
            Amount = amount;
            AmountOperator = amountOperator;
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(ts);
        }

        // Reset source
        public void ResetSource()
        {
            Source = null;
        }

        // Absorb, returns remaining damage/heal (only for absorb Aura)
        public int Absorb(int amount)
        {
            if (amount <= Amount) // Full absorb
            {
                Amount -= amount;
                return 0;
            }
            else // Partial absorb
            {
                int remaining = amount - Amount;
                Amount = 0;
                return remaining;
            }
        }

        // Refresh with a new aura
        public void Refresh(IAura aura)
        {
            StartTime = TimeHandler.CurrentTime;

            // Refresh aura values
            Amount = aura.Amount;
            PulseLeft = aura.PulseLeft;
        }

        // Change level, amount and pulseCount
        public void Modify(int? level, int? amount, TimeSpan? ts)
        {
            if (level.HasValue)
                Level = Math.Max(1, level.Value);
            if (amount.HasValue)
                Amount = amount.Value;
            if (ts.HasValue)
                PulseLeft = Pulse.FromTimeSpan(ts.Value);
        }

        // Change level, amount, pulseCount and ability
        public void Modify(int? level, int? amount, TimeSpan? ts, IAbility ability)
        {
            Modify(level, amount, ts);
            Ability = ability;
        }

        // Called when dispelled
        public virtual void OnDispelled(IEntity dispelSource)
        {
        }

        // Called when vanished
        public virtual void OnVanished()
        {
        }

        // Decrease pulse left
        public bool DecreasePulseLeft(int pulseCount)  // true if timed out
        {
            if (PulseLeft < 0)
                return false;
            PulseLeft = Math.Max(PulseLeft - pulseCount, 0);
            return PulseLeft == 0;
        }
    }
}
