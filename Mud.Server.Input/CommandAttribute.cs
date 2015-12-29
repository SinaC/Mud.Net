using System;
using System.Collections.Generic;

namespace Mud.Server.Input
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; private set; }
        public int Priority { get; set; } // Lower value means higher priority

        public CommandAttribute(string name)
        {
            Name = name;
            Priority = 999;
        }
    }

    public class CommandAttributeEqualityComparer : IEqualityComparer<CommandAttribute>
    {
        public bool Equals(CommandAttribute x, CommandAttribute y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(CommandAttribute obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
