using System.Collections.Generic;
using Mud.Domain;
using Mud.Network;

namespace Mud.Server
{
    public interface IServer
    {
        void Initialize(List<INetworkServer> networkServers);
        void Start();
        void Stop();

        void Shutdown(int seconds);
        void Quit(IPlayer player);
        void Delete(IPlayer player);
        void Promote(IPlayer player, AdminLevels level);
    }
}
