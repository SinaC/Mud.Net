namespace Mud.Server.Affects;

public class AffectInfo
{
    public string Name { get; }
    public Type AffectDataType { get; }

    public Type AffectType { get; }
    public AffectAttribute AffectAttribute { get; }

    public AffectInfo(Type affectType, string name, Type affectDataType, AffectAttribute affectAttribute)
    {
        AffectType = affectType;
        Name = name;
        AffectDataType = affectDataType;
        AffectAttribute = affectAttribute;
    }
}
