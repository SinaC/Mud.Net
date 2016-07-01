﻿using System;
using Mud.Server.Constants;

namespace Mud.Server.Aura
{
    public class Aura : IAura
    {
        #region IAura

        public IAbility Ability { get; }
        public ICharacter Source { get; private set; }
        public AuraModifiers Modifier { get; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; }
        public DateTime StartTime { get; private set; }
        public int TotalSeconds { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = Repository.Server.CurrentTime - StartTime;
                return TotalSeconds - (int)Math.Ceiling(ts.TotalSeconds);
            }
        }

        #endregion

        public Aura(IAbility ability, ICharacter source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds)
        {
            StartTime = Repository.Server.CurrentTime;

            Ability = ability;
            Source = source;
            Modifier = modifier;
            Amount = amount;
            AmountOperator = amountOperator;
            TotalSeconds = totalSeconds;
        }

        // Reset source
        public void ResetSource()
        {
            Source = null;
        }

        // Absorb, returns remaining damage (only for absorb Aura)
        public int Absorb(int damage)
        {
            if (damage <= Amount) // Full absorb
            {
                Amount -= damage;
                return 0;
            }
            else // Partial absorb
            {
                int remaining = damage - Amount;
                Amount = 0;
                return remaining;
            }
        }

        // Refresh with a new aura
        public void Refresh(IAura aura)
        {
            StartTime = Repository.Server.CurrentTime;

            // Refresh aura values
            Amount = aura.Amount;
            TotalSeconds = aura.TotalSeconds;
        }
    }
}
