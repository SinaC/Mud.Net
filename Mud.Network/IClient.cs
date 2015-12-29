namespace Mud.Network
{
    public delegate void DataReceivedEventHandler(string data);
    public delegate void DisconnectedEventHandler();

    public interface IClient
    {
        event DataReceivedEventHandler DataReceived;
        event DisconnectedEventHandler Disconnected;

        bool ColorAccepted { get; set; }

        void WriteData(string data);
        void Disconnect();
    }
}
