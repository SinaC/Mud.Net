namespace Mud.Network
{
    // TODO: should be able to tell when a client connects/disconnects
    public delegate void NewClientConnectedEventHandler(IClient client);
    public delegate void ClientDisconnectedEventHandler(IClient client);

    public interface INetworkServer
    {
        event NewClientConnectedEventHandler NewClientConnected;
        event ClientDisconnectedEventHandler ClientDisconnected;

        void Initialize();
        void Start();
        void Stop();
    }
}
