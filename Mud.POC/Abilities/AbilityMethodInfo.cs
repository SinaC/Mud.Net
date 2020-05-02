using System.Reflection;

namespace Mud.POC.Abilities
{
    public class AbilityMethodInfo
    {
        public AbilityAttribute Attribute { get; }
        public MethodInfo MethodInfo { get; }

        public AbilityMethodInfo(AbilityAttribute attribute, MethodInfo methodInfo)
        {
            Attribute = attribute;
            MethodInfo = methodInfo;
        }
    }
}
