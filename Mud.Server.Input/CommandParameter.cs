using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Input
{
    public class CommandParameter
    {
        public static readonly CommandParameter EmptyCommand = new CommandParameter();
        public static readonly CommandParameter InvalidCommand = new CommandParameter();
        public static readonly CommandParameter IsAllCommand = new CommandParameter(String.Empty, true);

        public bool IsAll { get; } // all.xxx
        public int Count { get; }
        public string Value { get; }

        public List<string> Tokens { get; }

        public bool IsNumber
        {
            get
            {
                int n;
                return int.TryParse(Value, out n);
            }
        }

        public int AsNumber {
            get
            {
                int intValue;
                int.TryParse(Value, out intValue);
                return intValue;
            }
        }

        private CommandParameter()
        {
            Tokens = new List<string>();
        }

        public CommandParameter(string value, int count)
        {
            Value = value;
            Count = count;
            Tokens = value.Split(' ').ToList();
        }

        public CommandParameter(string value, bool isAll)
        {
            Value = value;
            IsAll = isAll;
            Tokens = value.Split(' ').ToList();
        }

        public override string ToString()
        {
            return Count <= 1
                ? Value
                : Count + "." + Value;
        }
    }
}
