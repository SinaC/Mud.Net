namespace Mud.Network.Interfaces
{
    public delegate void DataReceivedEventHandler(IClient client, string data);

    public interface IClient
    {
        event DataReceivedEventHandler DataReceived;

        bool IsConnected { get; }
        bool ColorAccepted { get; set; }

        void EchoOff();
        void EchoOn();

        void WriteData(string data);
        void Disconnect();
    }
}
