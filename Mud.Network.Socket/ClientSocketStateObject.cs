using System.Text;
using Mud.Server;

namespace Mud.Network.Socket
{
    // State object for reading client data asynchronously
    internal class ClientSocketStateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 32;
        // Client socket.
        public System.Net.Sockets.Socket ClientSocket { get; set; }
        // Receive buffer.
        public byte[] Buffer { get; set; }
        // Received data string.
        public StringBuilder Command { get; set; }

        // Client
        public IPlayer Client { get; set; }

        public ClientSocketStateObject()
        {
            Buffer = new byte[BufferSize];
            Command = new StringBuilder();
            ClientSocket = null;
            Client = null;
        }
    }
}
