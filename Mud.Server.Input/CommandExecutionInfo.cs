namespace Mud.Server.Input
{
    public abstract class CommandExecutionInfo
    {
        public static SyntaxAttribute DefaultSyntaxCommandAttribute = new SyntaxAttribute("[cmd]");

        public CommandAttribute CommandAttribute { get; }

        public SyntaxAttribute SyntaxAttribute { get; }

        protected CommandExecutionInfo(CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
        {
            CommandAttribute = commandAttribute;
            SyntaxAttribute = syntaxAttribute;
        }

    }
}
