﻿using System;
using Mud.Domain;

namespace Mud.Server.GameAction
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public const int DefaultPriority = 500;
        public const string DefaultCategory = "";

        public string Name { get; }
        public int Priority { get; set; } // Lower value means higher priority
        public bool Hidden { get; set; } // Not displayed in command list
        public bool NoShortcut { get; set; } // Command must be fully typed
        public bool AddCommandInParameters { get; set; } // Command must be added in parameter list
        public string[] Categories { get; set; }

        public CommandAttribute(string name, params string[] categories)
        {
            Name = name.ToLowerInvariant();
            Priority = DefaultPriority;
            Hidden = false;
            Categories = categories?.Length == 0
                ? new[] { DefaultCategory }
                : categories;
            NoShortcut = false;
            AddCommandInParameters = false;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CharacterCommandAttribute : CommandAttribute
    {
        public Positions MinPosition { get; set; }

        public CharacterCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
            MinPosition = Positions.Dead;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PlayableCharacterCommandAttribute : CharacterCommandAttribute // Must be impersonated
    {
        public PlayableCharacterCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PlayerCommandAttribute : CommandAttribute
    {
        public bool MustBeImpersonated { get; set; }
        public bool CannotBeImpersonated { get; set; }

        public PlayerCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AdminCommandAttribute : PlayerCommandAttribute
    {
        public AdminLevels MinLevel { get; set; }

        public AdminCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SyntaxAttribute : Attribute
    {
        public string[] Syntax { get; }

        public SyntaxAttribute(params string[] syntax)
        {
            Syntax = syntax;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AliasAttribute : Attribute
    {
        public string Alias { get; }

        public AliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public enum CommandExecutionResults 
    {
        Ok,
        SyntaxError, // will display command syntax
        SyntaxErrorNoDisplay, // will NOT display command syntax
        TargetNotFound, // item/character/room/... has not been found
        InvalidParameter, // parameter invalid such as negative number
        InvalidTarget, // target cannot be used for this command
        NoExecution, //
        Error // will display an error in log and AfterCommand will not be executed
    }
}
