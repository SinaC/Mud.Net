using System.Reflection;

namespace Mud.Server.Affects;

public class AffectDefinition
{
    public string Name { get; }
    public Type AffectType { get; }
    public Type AffectDataType { get; }
    public MethodInfo InitializeMethod { get; }

    public AffectDefinition(Type affectType, string name, Type affectDataType, MethodInfo initializeMethod)
    {
        AffectType = affectType;
        Name = name;
        AffectDataType = affectDataType;
        InitializeMethod = initializeMethod;
    }
}
