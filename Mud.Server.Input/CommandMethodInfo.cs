using System.Reflection;

namespace Mud.Server.Input
{
    public class CommandMethodInfo : CommandExecutionInfo
    {
        public MethodInfo MethodInfo { get; }

        public CommandMethodInfo(MethodInfo methodInfo, CommandAttribute attribute, SyntaxAttribute syntax)
            : base(attribute, syntax)
        {
            MethodInfo = methodInfo;
        }
    }
}
