using Mud.Domain.SerializationData;

namespace Mud.Server.Interfaces.Affect;

public interface ICustomAffect : IAffect
{
    void Initialize(AffectDataBase data);
}
