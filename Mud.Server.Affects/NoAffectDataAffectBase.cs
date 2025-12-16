using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect;
using System.Reflection;
using System.Text;

namespace Mud.Server.Affects;

public abstract class NoAffectDataAffectBase : IAffect
{
    protected string AffectName { get; }

    protected NoAffectDataAffectBase()
    {
        var affectType = GetType();
        var affectAttribute = affectType.GetCustomAttribute<AffectAttribute>() ?? throw new Exception($"no AffectAttribute found for Affect {affectType.FullName}");
        AffectName = affectAttribute.Name;
    }

    public abstract void Append(StringBuilder sb);

    public AffectDataBase MapAffectData()
        => new NoAffectData { AffectName = AffectName };
}