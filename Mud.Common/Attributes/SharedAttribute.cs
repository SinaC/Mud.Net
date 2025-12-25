namespace Mud.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SharedAttribute : Attribute
{
    public SharedAttribute() // to be used with Export to expose a singleton service
    {
    }
}
