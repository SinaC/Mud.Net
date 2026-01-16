using Mud.Domain.SerializationData.Avatar;
using System.Text;

namespace Mud.Server.Interfaces.Affect;

public interface IAffect
{
    void Append(StringBuilder sb);

    AffectDataBase MapAffectData();
}
