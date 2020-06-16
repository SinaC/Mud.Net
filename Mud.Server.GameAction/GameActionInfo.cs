using Mud.Server.Input;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Linq;
using System.Reflection;

namespace Mud.Server.GameAction
{
    public class GameActionInfo : CommandExecutionInfo, IGameActionInfo
    {
        public Type CommandExecutionType { get; }

        public string Name { get; }
        public int Priority { get; }
        public bool Hidden { get; }
        public bool NoShortcut { get; }
        public bool AddCommandInParameters { get; }
        public string[] Categories { get; }

        public string[] Syntax { get; }

        public GameActionInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandAttribute, syntaxAttribute)
        {
            CommandExecutionType = commandExecutionType;

            Name = commandAttribute.Name;
            Priority = commandAttribute.Priority;
            Hidden = commandAttribute.Hidden;
            NoShortcut = commandAttribute.NoShortcut;
            AddCommandInParameters = commandAttribute.AddCommandInParameters;
            Categories = commandAttribute.Categories;

            Syntax = syntaxAttribute.Syntax;
        }

        public static IGameActionInfo Create(Type type) // TODO: replace with ctor when CommandExecutionInfo and CommandMethodInfo will be removed
        {
            CommandAttribute commandAttribute = type.GetCustomAttributes<CommandAttribute>().FirstOrDefault();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;
            return new GameActionInfo(type, commandAttribute, syntaxAttribute);
        }
    }
}
