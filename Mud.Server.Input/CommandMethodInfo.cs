using System.Reflection;

namespace Mud.Server.Input
{
    public class CommandMethodInfo
    {
        public CommandAttribute Attribute { get; private set; }
        public MethodInfo MethodInfo { get; private set; }

        public CommandMethodInfo(CommandAttribute attribute, MethodInfo methodInfo)
        {
            Attribute = attribute;
            MethodInfo = methodInfo;
        }
    }
}
