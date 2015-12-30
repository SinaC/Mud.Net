namespace Mud.Network
{
    //public delegate void DataReceivedEventHandler(string data);
    public delegate void DisconnectedEventHandler();

    public interface IClient
    {
        //event DataReceivedEventHandler DataReceived;
        event DisconnectedEventHandler Disconnected;

        bool ColorAccepted { get; set; }

        string ReadData(); // Try to read data from client; if no data, null is return

        void WriteData(string data);
        void Disconnect();
    }
}
