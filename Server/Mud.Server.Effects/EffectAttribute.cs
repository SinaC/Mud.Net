using Mud.Common.Attributes;

namespace Mud.Server.Effects;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EffectAttribute : ExportAttribute // every effect will be exported without ContractType
{
    public string Name { get; }

    public EffectAttribute(string name)
    {
        Name = name;
    }
}
