using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Affect;

public interface IAffectManager
{
    IAffect? CreateInstance(string name);
    IAffect? CreateInstance(AffectDataBase data);
}
