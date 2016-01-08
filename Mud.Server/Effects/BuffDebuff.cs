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

        public int SecondsLeft
        {
            get
            {
                TimeSpan ts = Server.Server.Instance.CurrentTime - StartTime;
                return TotalSeconds - (int)Math.Ceiling(ts.TotalSeconds);
            }
        }

        public BuffDebuff(string name, AttributeTypes attributeType, int amount, AmountOperators amountOperator, int totalSeconds)
        {
            StartTime = Server.Server.Instance.CurrentTime;

            Name = name;
            AttributeType = attributeType;
            Amount = amount;
            AmountOperator = amountOperator;
            TotalSeconds = totalSeconds;
        }
    }
}
