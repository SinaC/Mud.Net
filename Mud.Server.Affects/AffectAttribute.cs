using Mud.Common.Attributes;

namespace Mud.Server.Affects;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AffectAttribute : ExportAttribute // every affect will be exported without ContractType
{
    public string Name { get; }
    public Type AffectDataType { get; }

    public AffectAttribute(string name, Type affectDataType)
    {
        Name = name;
        AffectDataType = affectDataType;
    }
}
