using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.GameAction
{
    public class GameActionInfo : IGameActionInfo
    {
        public static SyntaxAttribute DefaultSyntaxCommandAttribute = new SyntaxAttribute("[cmd]");

        public string Name { get; }
        public int Priority { get; }
        public bool Hidden { get; }
        public bool NoShortcut { get; }
        public bool AddCommandInParameters { get; }
        public string[] Categories { get; }

        public string[] Syntax { get; }

        public string[] Aliases { get; }

        public Type CommandExecutionType { get; }


        public GameActionInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
        {
            CommandExecutionType = commandExecutionType;

            Name = commandAttribute.Name;
            Priority = commandAttribute.Priority;
            Hidden = commandAttribute.Hidden;
            NoShortcut = commandAttribute.NoShortcut;
            AddCommandInParameters = commandAttribute.AddCommandInParameters;
            Categories = commandAttribute.Categories;

            Syntax = syntaxAttribute.Syntax;

            Aliases = aliasAttributes?.Select(x => x.Alias).ToArray();
        }
    }
}
