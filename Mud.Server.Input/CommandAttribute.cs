using System;
using System.Collections.Generic;

namespace Mud.Server.Input
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)] // TODO: multiple category
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public int Priority { get; set; } // Lower value means higher priority
        public bool Hidden { get; set; } // Not displayed in command list
        public string Category { get; set; } // TODO: use category: Info/Communication/Movement/...
        public bool AddCommandInParameters { get; set; } // Command must be added in parameter list

        public CommandAttribute(string name)
        {
            Name = name;
            Priority = 999;
            Hidden = false;
            Category = String.Empty;
            AddCommandInParameters = false;
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
