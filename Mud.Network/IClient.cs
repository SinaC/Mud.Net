namespace Mud.Network
{
    public delegate void DataReceivedEventHandler(IClient client, string data);

    public interface IClient
    {
        event DataReceivedEventHandler DataReceived;

        bool ColorAccepted { get; set; }

        void EchoOff();
        void EchoOn();

        void WriteData(string data);
        void Disconnect();
    }
}
