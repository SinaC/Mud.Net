using System.Reflection;

namespace Mud.Server.Affects;

public class AffectDefinition
{
    public string Name { get; }
    public Type AffectType { get; }
    public bool NoAffectData { get; }
    public Type? AffectDataType { get; }
    public MethodInfo? InitializeMethod { get; }

    public AffectDefinition(Type affectType, string name, Type affectDataType, MethodInfo initializeMethod)
    {
        AffectType = affectType;
        Name = name;
        NoAffectData = false;
        AffectDataType = affectDataType;
        InitializeMethod = initializeMethod;
    }

    public AffectDefinition(Type affectType, string name)
    {
        AffectType = affectType;
        Name = name;
        NoAffectData = true;
    }
}
