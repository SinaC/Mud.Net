using System;
using Mud.Server.Constants;

namespace Mud.Server.Effects
{
    public class BuffDebuff : IBuffDebuff
    {
        public string Name { get; private set; }
        public AttributeTypes AttributeType { get; private set; }
        public int Amount { get; private set; }
        public AmountOperators AmountOperator { get; private set; }
        public DateTime StartTime { get; private set; }
        public int TotalSeconds { get; private set; }

        public BuffDebuff(string name, AttributeTypes attributeType, int amount, AmountOperators amountOperator, int totalSeconds)
        {
            StartTime = DateTime.Now;

            Name = name;
            AttributeType = attributeType;
            Amount = amount;
            AmountOperator = amountOperator;
            TotalSeconds = totalSeconds;
        }
    }
}
