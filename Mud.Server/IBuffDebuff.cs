﻿using System;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IBuffDebuff
    {
        // Name
        string Name { get; }

        // Attributes
        AttributeTypes AttributeType { get; }

        // Amount + %/fixed
        int Amount { get; }
        AmountOperators AmountOperator { get; }

        // Start time
        DateTime StartTime { get; }

        // Total left
        int TotalSeconds { get; }

        // Seconds left
        int SecondsLeft { get; }
    }
}