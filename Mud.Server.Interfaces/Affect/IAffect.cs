using Mud.Domain;
using System.Text;

namespace Mud.Server.Interfaces.Affect
{
    public interface IAffect : IRegistrable
    {
        void Append(StringBuilder sb);

        AffectDataBase MapAffectData();
    }
}
