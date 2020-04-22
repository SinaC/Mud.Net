using System;
using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Input
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)] // TODO: multiple category
    public class CommandAttribute : Attribute
    {
        public const int DefaultPriority = 0;

        public string Name { get; }
        public int Priority { get; set; } // Lower value means higher priority
        public bool Hidden { get; set; } // Not displayed in command list
        public string Category { get; set; }
        public bool NoShortcut { get; set; } // Command must be fully typed
        public bool AddCommandInParameters { get; set; } // Command must be added in parameter list

        public CommandAttribute(string name)
        {
            Name = name;
            Priority = DefaultPriority;
            Hidden = false;
            Category = string.Empty;
            NoShortcut = false;
            AddCommandInParameters = false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PlayableCharacterCommandAttribute : CommandAttribute // Must be impersonated
    {
        public PlayableCharacterCommandAttribute(string name)
            : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PlayerCommandAttribute : CommandAttribute
    {
        public bool MustBeImpersonated { get; set; }
        public bool CannotBeImpersonated { get; set; }

        public PlayerCommandAttribute(string name)
            : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AdminCommandAttribute : PlayerCommandAttribute
    {
        public AdminLevels MinLevel { get; set; }

        public AdminCommandAttribute(string name) 
            : base(name)
        {
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
