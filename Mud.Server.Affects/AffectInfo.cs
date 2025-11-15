using Mud.Server.Interfaces.Affect;

namespace Mud.Server.Affects;

public class AffectInfo : IAffectInfo
{
    public string Name { get; }
    public Type AffectDataType { get; }

    public Type AffectType { get; }

    public AffectInfo(Type affectType, string name, Type affectDataType)
    {
        AffectType = affectType;
        Name = name;
        AffectDataType = affectDataType;
    }
}
