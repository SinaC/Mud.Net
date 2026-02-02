using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.Attributes;

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

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AffectNoDataAttribute : AffectAttribute
{
    public AffectNoDataAttribute(string name)
        : base(name, typeof(NoAffectData))
    {
    }
}