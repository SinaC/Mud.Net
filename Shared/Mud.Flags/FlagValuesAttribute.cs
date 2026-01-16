using Mud.Common.Attributes;

namespace Mud.Flags;


[AttributeUsage(AttributeTargets.Class,  AllowMultiple = false)]
public class FlagValuesAttribute : ExportAttribute
{
    public Type FlagInterfaceType { get; }

    public FlagValuesAttribute(Type contractType, Type flagInterfaceType)
        : base(contractType)
    {
        FlagInterfaceType = flagInterfaceType;
    }
}
