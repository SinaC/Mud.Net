using System.Text;

namespace Mud.Network.Socket
{
    // State object for reading client data asynchronously
    internal class ClientSocketStateObject : IClient
    {
        // Size of receive buffer.
        public const int BufferSize = 32;
        // Client socket.
        public System.Net.Sockets.Socket ClientSocket { get; set; }
        // Receive buffer.
        public byte[] Buffer { get; set; }
        // Received data string.
        public StringBuilder Command { get; set; }

        public SocketServer Server { get; private set; }

        public ClientSocketStateObject(SocketServer server)
        {
            Server = server;
            Buffer = new byte[BufferSize];
            Command = new StringBuilder();
            ClientSocket = null;
        }

        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;

        public void WriteData(string data)
        {
            Server.Send(ClientSocket, data);
        }

        public void Disconnect()
        {
            Server.CloseConnection(this);
        }

        public void OnDataReceived(string data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }

        public void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }
    }
}
