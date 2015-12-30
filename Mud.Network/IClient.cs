namespace Mud.Network
{
    public delegate void DataReceivedEventHandler(string data);
    public delegate void DisconnectedEventHandler();

    public interface IClient
    {
        event DataReceivedEventHandler DataReceived; // If AsynchronousReceive is true, DataReceived will be raised when a data is received
        event DisconnectedEventHandler Disconnected;

        bool ColorAccepted { get; set; }
        bool AsynchronousReceive { get; } // if true, DataReceived will be raised when a data is received, otherwise ReadData must be called periodically

        string ReadData(); // If AsynchronousReceive is false, try to read data from client; if no data or AsynchronousReceive is true, null is return

        void WriteData(string data);
        void Disconnect();
    }
}
