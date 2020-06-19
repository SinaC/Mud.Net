using Mud.Server.Interfaces.GameAction;
using System;
using System.Linq;
using System.Reflection;

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

        public Type CommandExecutionType { get; }


        protected GameActionInfo(CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
        {
            Name = commandAttribute.Name;
            Priority = commandAttribute.Priority;
            Hidden = commandAttribute.Hidden;
            NoShortcut = commandAttribute.NoShortcut;
            AddCommandInParameters = commandAttribute.AddCommandInParameters;
            Categories = commandAttribute.Categories;

            Syntax = syntaxAttribute.Syntax;
        }

        public GameActionInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
            : this(commandAttribute, syntaxAttribute)
        {
            CommandExecutionType = commandExecutionType;
        }

        // TODO: remove: only used in Tests
        public static IGameActionInfo Create(Type type) // TODO: replace with ctor when CommandExecutionInfo and CommandMethodInfo will be removed
        {
            CommandAttribute commandAttribute = type.GetCustomAttributes<CommandAttribute>().FirstOrDefault();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? DefaultSyntaxCommandAttribute;
            return new GameActionInfo(type, commandAttribute, syntaxAttribute);
        }
    }
}
