namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HelpAttribute(string help) : Attribute
{
    public string Help { get; } = help;
}
