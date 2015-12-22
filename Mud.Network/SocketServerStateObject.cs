using System.Net.Sockets;
using System.Text;

namespace Mud.Network
{
    // State object for reading client data asynchronously
    internal class SocketServerStateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 32;
        // Client socket.
        public Socket ClientSocket { get; set; }
        // Receive buffer.
        public byte[] Buffer { get; set; }
        // Received data string.
        public StringBuilder Command { get; set; }

        // Client
        public IClient Client { get; set; }

        public SocketServerStateObject()
        {
            Buffer = new byte[BufferSize];
            Command = new StringBuilder();
            ClientSocket = null;
            Client = null;
        }
    }
}
