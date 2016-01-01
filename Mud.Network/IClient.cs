namespace Mud.Network
{
    public delegate void DataReceivedEventHandler(IClient client, string data);
    public delegate void DisconnectedEventHandler(IClient client);

    public interface IClient
    {
        event DataReceivedEventHandler DataReceived;
        event DisconnectedEventHandler Disconnected;

        bool ColorAccepted { get; set; }

        void EchoOff();
        void EchoOn();

        void WriteData(string data);
        void Disconnect();
    }
}
