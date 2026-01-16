using Mud.Common.Attributes;

namespace Mud.POC.DynamicCommand
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GenericCommandAttribute : ExportAttribute
    {
    }
}
