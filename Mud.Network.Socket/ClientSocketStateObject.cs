using System.Text;

namespace Mud.Network.Socket
{
    internal enum ClientStates
    {
        Handshaking,
        Connected,
    }

    // State object for reading client data asynchronously
    internal class ClientSocketStateObject : IClient
    {
        private static byte[] EchoOffData = new[] { (byte)0xFF, (byte)0xFB, (byte)0x01 };
        private static byte[] EchoOnData = new[] { (byte)0xFF, (byte)0xFC, (byte)0x01 };

        // Size of receive buffer.
        public const int BufferSize = 256;
        // Client socket.
        public System.Net.Sockets.Socket ClientSocket { get; set; }
        // Receive buffer.
        public byte[] Buffer { get; set; }
        // Received data string.
        public StringBuilder Command { get; set; }
        // Server
        public SocketServer Server { get; private set; }
        // State
        public ClientStates State { get; set; }

        public ClientSocketStateObject(SocketServer server)
        {
            Server = server;
            Buffer = new byte[BufferSize];
            Command = new StringBuilder();
            ClientSocket = null;
            ColorAccepted = true; // by default
            State = ClientStates.Handshaking;
        }

        public void OnDataReceived(string data)
        {
            if (DataReceived != null)
                DataReceived(this, data);
        }

        #region IClient

        public event DataReceivedEventHandler DataReceived;

        public bool ColorAccepted { get; set; }

        public void EchoOff() // http://stackoverflow.com/questions/6380257/how-can-i-mask-user-input-when-telneting
        {
            Server.SendData(this, EchoOffData);
        }

        public void EchoOn() // http://stackoverflow.com/questions/6380257/how-can-i-mask-user-input-when-telneting
        {
            Server.SendData(this, EchoOnData);
        }

        public void WriteData(string data)
        {
            Server.SendData(this, data);
        }

        public void Disconnect()
        {
            Server.CloseConnection(this);
        }

        #endregion
    }
}
