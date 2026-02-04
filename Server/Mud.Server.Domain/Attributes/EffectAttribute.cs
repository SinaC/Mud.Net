using Mud.Common.Attributes;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EffectAttribute(string name) : ExportAttribute // every effect will be exported without ContractType
{
    public string Name { get; } = name.ToLowerInvariant();
}
