using System.Collections.Generic;
using Mud.Network;

namespace Mud.Server.Interfaces
{
    public interface IServer
    {
        void Initialize(List<INetworkServer> networkServers);
        void Start();
        void Stop();
        void Dump();
    }
}
