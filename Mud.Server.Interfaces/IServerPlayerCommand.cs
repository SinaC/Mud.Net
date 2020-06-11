using Mud.Server.Interfaces.Player;

namespace Mud.Server.Interfaces
{
    public interface IServerPlayerCommand
    {
        void Quit(IPlayer player);
        void Delete(IPlayer player);

    }
}
