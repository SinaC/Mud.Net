using Mud.Server.Interfaces.GameAction;
using System.Reflection;

namespace Mud.Server.GameAction
{
    public class CommandMethodInfo : CommandExecutionInfo, ICommandMethodInfo
    {
        public MethodInfo MethodInfo { get; }

        public CommandMethodInfo(MethodInfo methodInfo, CommandAttribute attribute, SyntaxAttribute syntax)
            : base(attribute, syntax)
        {
            MethodInfo = methodInfo;
        }
    }
}
