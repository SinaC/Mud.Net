using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.GroupsPetsFollowers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CharacterCommandAttribute : CommandAttribute
    {

        public CharacterCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PlayableCharacterCommandAttribute : CharacterCommandAttribute
    {

        public PlayableCharacterCommandAttribute(string name, params string[] categories)
            : base(name, categories)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SyntaxAttribute : Attribute
    {
        public string[] Syntax { get; }

        public SyntaxAttribute(params string[] syntax)
        {
            Syntax = syntax;
        }
    }

}
