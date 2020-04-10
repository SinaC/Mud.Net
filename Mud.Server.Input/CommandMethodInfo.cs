using System.Reflection;

namespace Mud.Server.Input
{
    public class CommandMethodInfo
    {
        public CommandAttribute Attribute { get; }
        public MethodInfo MethodInfo { get; }

        public CommandMethodInfo(CommandAttribute attribute, MethodInfo methodInfo)
        {
            Attribute = attribute;
            MethodInfo = methodInfo;
        }
    }
}
