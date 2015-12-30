using System.Collections.Concurrent;
using System.Text;
using System.Threading;

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
        // Server
        public SocketServer Server { get; private set; }
        // First must be eaten
        public bool FirstInput { get; set; }

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<string> _receiveQueue;

        public ClientSocketStateObject(SocketServer server, CancellationTokenSource cancellationTokenSource)
        {
            Server = server;
            Buffer = new byte[BufferSize];
            Command = new StringBuilder();
            ClientSocket = null;
            FirstInput = true;
            ColorAccepted = true; // by default
            _cancellationTokenSource = cancellationTokenSource;
            _receiveQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        }

        //public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;

        public bool ColorAccepted { get; set; }

        public string ReadData()
        {
            string data;
            bool taken = _receiveQueue.TryTake(out data, 10, _cancellationTokenSource.Token);
            return taken ? data : null;
        }

        public void WriteData(string data)
        {
            Server.Send(this, data);
        }

        public void Disconnect()
        {
            Server.CloseConnection(this);
        }

        public void OnDataReceived(string data)
        {
            //if (DataReceived != null)
            //    DataReceived(data);
            _receiveQueue.Add(data);
        }

        public void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }
    }
}
