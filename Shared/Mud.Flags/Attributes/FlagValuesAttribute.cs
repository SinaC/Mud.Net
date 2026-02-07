using Mud.Common.Attributes;

namespace Mud.Flags.Attributes;


[AttributeUsage(AttributeTargets.Class,  AllowMultiple = false)]
public class FlagValuesAttribute(Type contractType, Type flagInterfaceType) : ExportAttribute(contractType)
{
    public Type FlagInterfaceType { get; } = flagInterfaceType;
}
