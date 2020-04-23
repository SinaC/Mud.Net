using System.Reflection;

namespace Mud.Server.Input
{
    public class CommandMethodInfo
    {
        public static SyntaxAttribute DefaultSyntaxCommandAttribute = new SyntaxAttribute("[cmd]");

        public CommandAttribute Attribute { get; }
        public MethodInfo MethodInfo { get; }
        public SyntaxAttribute Syntax { get; }

        public CommandMethodInfo(CommandAttribute attribute, MethodInfo methodInfo)
            : this(attribute, methodInfo, DefaultSyntaxCommandAttribute)
        {
        }

        public CommandMethodInfo(CommandAttribute attribute, MethodInfo methodInfo, SyntaxAttribute syntax)
        {
            Attribute = attribute;
            MethodInfo = methodInfo;
            Syntax = syntax ?? DefaultSyntaxCommandAttribute;
        }
    }
}
