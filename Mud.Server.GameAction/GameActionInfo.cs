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

        public string Name => CommandAttribute.Name;
        public int Priority => CommandAttribute.Priority;
        public bool Hidden => CommandAttribute.Hidden;
        public bool NoShortcut => CommandAttribute.NoShortcut;
        public bool AddCommandInParameters => CommandAttribute.AddCommandInParameters;
        public string[] Categories => CommandAttribute.Categories;

        public string[] Syntax => SyntaxAttribute.Syntax;

        public GameActionInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandAttribute, syntaxAttribute)
        {
            CommandExecutionType = commandExecutionType;
        }

        public static IGameActionInfo Create(Type type) // TODO: replace with ctor when CommandExecutionInfo and CommandMethodInfo will be removed
        {
            CommandAttribute commandAttribute = type.GetCustomAttributes<CommandAttribute>().FirstOrDefault();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;
            return new GameActionInfo(type, commandAttribute, syntaxAttribute);
        }
    }
}
