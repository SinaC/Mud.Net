namespace Mud.Network
{
    // TODO: should be able to tell when a client connects/disconnects
    public delegate void NewClientConnectedEventHandler(IClient client);

    public interface INetworkServer
    {
        event NewClientConnectedEventHandler NewClientConnected;

        bool AsynchronousReceive { get; }

        void Initialize();
        void Start();
        void Stop();

        void Broadcast(string data);
    }
}
