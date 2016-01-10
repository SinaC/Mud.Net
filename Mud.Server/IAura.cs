using System;
using Mud.Server.Constants;

namespace Mud.Server
{
    // TODO: single target or group
    public interface IAura
    {
        // Name
        string Name { get; }

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

        // Change amount, return true if amount <= 0
        bool ChangeAmount(int delta);
    }
}
