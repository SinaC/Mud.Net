using System;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Aura
{
    // TODO: Aura with multiple modifiers
    public class Aura : IAura
    {
        private const int NoAbilityId = -1;

        private IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        public Aura(IAbility ability, IEntity source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int level, TimeSpan ts)
        {
            Ability = ability;
            Source = source;
            Modifier = modifier;
            Amount = amount;
            AmountOperator = amountOperator;
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(ts);
        }

        public Aura(AuraData auraData)
        {
            if (auraData.AbilitiId == NoAbilityId)
                Ability = null;
            else
            {
                Ability = AbilityManager.Abilities.FirstOrDefault(x => x.Id == auraData.AbilitiId);
                if (Ability == null)
                    Log.Default.WriteLine(LogLevels.Error, "Aura ability id {0} doesn't exist anymore", auraData.AbilitiId);
            }
            // TODO: source
            Modifier = auraData.Modifier;
            Amount = auraData.Amount;
            AmountOperator = auraData.AmountOperator;
            Level = auraData.Level;
            PulseLeft = auraData.PulseLeft;
        }

        #region IAura

        public IAbility Ability { get; private set; }
        public IEntity Source { get; private set; }
        public AuraModifiers Modifier { get; }
        public int Amount { get; private set; } // can be a flag
        public AmountOperators AmountOperator { get; } // Amount is a flag is AmountOperator is Flags
        public int Level { get; private set; }
        public int PulseLeft { get; private set; }

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

        //
        public AuraData MapAuraData()
        {
            return new AuraData
            {
                AbilitiId = Ability?.Id ?? NoAbilityId,
                // TODO: source
                Modifier = Modifier,
                Amount = Amount,
                AmountOperator = AmountOperator,
                Level = Level,
                PulseLeft = PulseLeft
            };
        }

        #endregion
    }
}
