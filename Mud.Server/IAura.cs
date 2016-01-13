using System;
using Mud.Server.Constants;
using Mud.Server.World;

namespace Mud.Server
{
    public interface IAura
    {
        // Ability
        IAbility Ability { get; }

        // Modifier
        AuraModifiers Modifier { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Start time
        DateTime StartTime { get; }

        // Total left
        int TotalSeconds { get; }

        // Seconds left
        int SecondsLeft { get; }

        // Absorb, returns remaining damage
        int Absorb(int damage);
    }
}
