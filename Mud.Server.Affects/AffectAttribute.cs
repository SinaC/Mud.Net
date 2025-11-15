namespace Mud.Server.Affects;

[AttributeUsage(AttributeTargets.Class)]
public class AffectAttribute : Attribute
{
    public string Name { get; }
    public Type AffectDataType { get; }

    public AffectAttribute(string name, Type affectDataType)
    {
        Name = name;
        AffectDataType = affectDataType;
    }
}
