using Mud.Server.Input;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Reflection;

namespace Mud.Server.Command
{
    public class CommandInfo : CommandExecutionInfo, ICommandInfo
    {
        public Type CommandExecutionType { get; }

        public string Name => CommandAttribute.Name;
        public int Priority => CommandAttribute.Priority;
        public bool Hidden => CommandAttribute.Hidden;
        public bool NoShortcut => CommandAttribute.NoShortcut;
        public bool AddCommandInParameters => CommandAttribute.AddCommandInParameters;
        public string[] Categories => CommandAttribute.Categories;

        public string[] Syntax => SyntaxAttribute.Syntax;

        public CommandInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandAttribute, syntaxAttribute)
        {
            CommandExecutionType = commandExecutionType;
        }

        public static CommandInfo Create(Type type) // TODO: replace with ctor when CommandExecutionInfo and CommandMethodInfo will be removed
        {
            CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;
            return new CommandInfo(type, commandAttribute, syntaxAttribute);
        }
    }
}
