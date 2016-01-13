﻿using System;
using Mud.Server.Constants;
using Mud.Server.World;

namespace Mud.Server.Aura
{
    // TODO: linked to an ability
    public class Aura : IAura
    {
        #region IAura

        public IAbility Ability { get; private set; }
        public AuraModifiers Modifier { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }
        public DateTime StartTime { get; private set; }
        public int TotalSeconds { get; private set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = Server.Server.Instance.CurrentTime - StartTime;
                return TotalSeconds - (int)Math.Ceiling(ts.TotalSeconds);
            }
        }

        #endregion

        public Aura(IAbility ability, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds)
        {
            StartTime = Server.Server.Instance.CurrentTime;

            Ability = ability;
            Modifier = modifier;
            Amount = amount;
            AmountOperator = amountOperator;
            TotalSeconds = totalSeconds;
        }

        // Absorb
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
    }
}
