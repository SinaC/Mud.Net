using Mud.Domain;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Interfaces
{
    public interface IServerAdminCommand
    {
        void Shutdown(int seconds);
        void Promote(IPlayer player, AdminLevels level);
    }
}
