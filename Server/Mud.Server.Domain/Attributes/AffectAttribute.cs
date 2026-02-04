using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AffectAttribute(string name, Type affectDataType) : ExportAttribute // every affect will be exported without ContractType
{
    public string Name { get; } = name.ToLowerInvariant();
    public Type AffectDataType { get; } = affectDataType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AffectNoDataAttribute(string name) : AffectAttribute(name, typeof(NoAffectData))
{
}