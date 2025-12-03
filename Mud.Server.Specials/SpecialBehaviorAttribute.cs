using Mud.Common.Attributes;

namespace Mud.Server.Specials
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpecialBehaviorAttribute : ExportAttribute // every special behavior will be exported without ContractType
    {
        public string Name { get; }

        public SpecialBehaviorAttribute(string name)
        {
            Name = name;
        }
    }
}
