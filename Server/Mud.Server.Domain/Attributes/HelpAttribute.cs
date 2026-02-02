namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HelpAttribute : Attribute
{
    public string Help { get; }

    public HelpAttribute(string help)
    {
        Help = help;
    }
}
