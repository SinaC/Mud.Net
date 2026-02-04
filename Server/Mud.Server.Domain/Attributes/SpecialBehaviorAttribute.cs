using Mud.Common.Attributes;

namespace Mud.Server.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SpecialBehaviorAttribute(string name) : ExportAttribute // every special behavior will be exported without ContractType
    {
        public string Name { get; } = name.ToLowerInvariant();
    }
}
