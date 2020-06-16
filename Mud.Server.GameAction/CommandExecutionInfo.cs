using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public abstract class CommandExecutionInfo : ICommandExecutionInfo
    {
        public static SyntaxAttribute DefaultSyntaxCommandAttribute = new SyntaxAttribute("[cmd]");

        public CommandAttribute CommandAttribute { get; }

        public SyntaxAttribute SyntaxAttribute { get; }


        public string Name { get; }
        public int Priority { get; }
        public bool Hidden { get; }
        public bool NoShortcut { get; }
        public bool AddCommandInParameters { get; }
        public string[] Categories { get; }

        public string[] Syntax { get; }

        protected CommandExecutionInfo(CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
        {
            Name = commandAttribute.Name;
            Priority = commandAttribute.Priority;
            Hidden = commandAttribute.Hidden;
            NoShortcut = commandAttribute.NoShortcut;
            AddCommandInParameters = commandAttribute.AddCommandInParameters;
            Categories = commandAttribute.Categories;

            Syntax = syntaxAttribute.Syntax;

            CommandAttribute = commandAttribute;
            SyntaxAttribute = syntaxAttribute;
        }

    }
}
