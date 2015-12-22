using Mud.Server;

namespace Mud.Network
{
    // TODO: should be able to tell when a client connects/disconnects
    public interface INetworkServer
    {
        int Port { get; }

        void Initialize(int port);
        void Start();
        void Stop();

        void Send(IClient client, string data);
        void Broadcast(string data);
    }
}
