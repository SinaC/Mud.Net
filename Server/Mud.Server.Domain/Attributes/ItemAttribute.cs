using Mud.Common.Attributes;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ItemAttribute(Type blueprintType, Type itemDataType) : ExportAttribute // every affect will be exported without ContractType
{
    public Type BlueprintType { get; set; } = blueprintType;
    public Type ItemDataType { get; set; } = itemDataType;
}
