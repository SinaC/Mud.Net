using Mud.Domain;

namespace Mud.Server
{
    public interface IServerAdminCommand
    {
        void Shutdown(int seconds);
        void Promote(IPlayer player, AdminLevels level);
    }
}
