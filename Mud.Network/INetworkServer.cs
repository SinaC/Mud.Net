namespace Mud.Network
{
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
