using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Affect;

public interface IAffectManager
{
    int Count { get; }

    IAffect? CreateInstance(string name);
    IAffect? CreateInstance(AffectDataBase data);
}
