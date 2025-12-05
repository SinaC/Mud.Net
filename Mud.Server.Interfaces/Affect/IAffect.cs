using Mud.Domain.SerializationData;
using System.Text;

namespace Mud.Server.Interfaces.Affect;

public interface IAffect
{
    void Append(StringBuilder sb);

    AffectDataBase MapAffectData();
}
