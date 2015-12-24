using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;
using Mud.Server;

namespace Mud.Network.Socket
{
    // TODO: handling exception
    // TODO: client collection
    // TODO: forbid start/stop/... depending on server state

    internal enum ServerStatus
    {
        Creating,
        Created,
        Initializing,
        Initialized,
        Starting,
        Started,
        Stopping,
        Stopped
    }

    public class SocketServer : INetworkServer, IDisposable
    {
        private System.Net.Sockets.Socket _serverSocket;
        private ManualResetEvent _listenEvent;
        private Task _listenTask;
        private CancellationTokenSource _cancellationTokenSource;

        private ServerStatus _status;

        private readonly Func<IPlayer> _createClientFunc;
        private readonly List<ClientSocketStateObject> _serverStateObjects;

        public int Port { get; private set; }

        public SocketServer(Func<IPlayer> createClientFunc)
        {
            _status = ServerStatus.Creating;
            Log.Default.WriteLine(LogLevels.Info, "Server creating");

            _createClientFunc = createClientFunc;
            _serverStateObjects = new List<ClientSocketStateObject>();

            Log.Default.WriteLine(LogLevels.Info, "Server created");
            _status = ServerStatus.Created;
        }

        public void Initialize(int port)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server initializing");
            _status = ServerStatus.Initializing;

            Port = port;

            // Establish the local endpoint for the socket.
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            _serverSocket = new System.Net.Sockets.Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Listen task event
            _listenEvent = new ManualResetEvent(false);

            // Bind the socket to the local endpoint and listen for incoming connections.
            _serverSocket.Bind(localEndPoint);
            Log.Default.WriteLine(LogLevels.Info, "Server bound to " + hostName + " on port " + localEndPoint.Port);

            _status = ServerStatus.Initialized;
            Log.Default.WriteLine(LogLevels.Info, "Server initialized");
        }

        public void Start()
        {
            Log.Default.WriteLine(LogLevels.Info, "Server starting");

            _status = ServerStatus.Starting;

            _serverSocket.Listen(100);
            _cancellationTokenSource = new CancellationTokenSource();
            _listenTask = Task.Factory.StartNew(ListenTask, _cancellationTokenSource.Token);

            _status = ServerStatus.Started;

            Log.Default.WriteLine(LogLevels.Info, "Server started");
        }

        public void Stop()
        {
            Log.Default.WriteLine(LogLevels.Info, "Server stopping");

            _status = ServerStatus.Stopping;

            Broadcast("STOPPING SERVER NOW!!!");

            try
            {
                // Close clients socket
                List<ClientSocketStateObject> copy = _serverStateObjects.Select(x => x).ToList(); // make a copy, because CloseConnection modify collection
                foreach (ClientSocketStateObject serverStateObject in copy)
                    CloseConnection(serverStateObject);
                
                // Stop listen task
                _cancellationTokenSource.Cancel();
                _listenEvent.Set(); // wakeup thread
                _listenTask.Wait(2000);

                // Close server socket
                //_serverSocket.Shutdown(SocketShutdown.Both); // TODO: exception on this line when closing application
                _serverSocket.Close();
            }
            catch (AggregateException ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Aggregate exception while stopping. Exception: {0}", ex.Flatten());
            }

            Log.Default.WriteLine(LogLevels.Info, "Server stopped");

            _status = ServerStatus.Stopped;
        }

        public void Send(IPlayer client, string data)
        {
            // TODO: optimize and protect this search
            ClientSocketStateObject serverStateObject = _serverStateObjects.FirstOrDefault(x => x.Client == client);
            if (serverStateObject != null)
                Send((System.Net.Sockets.Socket) serverStateObject.ClientSocket, data);
        }

        public void Broadcast(string data)
        {
            foreach (ClientSocketStateObject serverStateObject in _serverStateObjects)
                Send((System.Net.Sockets.Socket) serverStateObject.ClientSocket, data);
        }

        private void ListenTask()
        {
            try
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop ListenTask requested");
                        break;
                    }

                    // Set the event to nonsignaled state.
                    _listenEvent.Reset();

                    Log.Default.WriteLine(LogLevels.Debug, "Waiting for connection");

                    // Start an asynchronous socket to listen for connections.
                    _serverSocket.BeginAccept(AcceptCallback, _serverSocket);

                    // Wait until a connection is made before continuing.
                    _listenEvent.WaitOne();
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "ListenTask exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "ListenTask stopped");
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the ListenTask to continue.
                _listenEvent.Set();

                // Get the socket that handles the client request.
                System.Net.Sockets.Socket listener = (System.Net.Sockets.Socket) ar.AsyncState;
                System.Net.Sockets.Socket clientSocket = listener.EndAccept(ar);

                Log.Default.WriteLine(LogLevels.Debug, "Client connected from " + ((IPEndPoint) clientSocket.RemoteEndPoint).Address);

                // Create the state object.
                ClientSocketStateObject state = new ClientSocketStateObject
                {
                    ClientSocket = clientSocket,
                    Client = _createClientFunc()
                };
                // Add it to 'client' collection
                _serverStateObjects.Add(state);
                //
                clientSocket.BeginReceive(state.Buffer, 0, ClientSocketStateObject.BufferSize, 0, ReadCallback, state);
            }
            catch (ObjectDisposedException)
            {
                // If server status is stopping/stopped: ok
                // else, throw
                if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped)
                    throw;
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                ClientSocketStateObject state = (ClientSocketStateObject) ar.AsyncState;
                System.Net.Sockets.Socket clientSocket = state.ClientSocket;

                // Read data from the client socket. 
                int bytesRead = clientSocket.EndReceive(ar);

                Log.Default.WriteLine(LogLevels.Debug, "Received " + bytesRead + " bytes from client at " + ((IPEndPoint) clientSocket.RemoteEndPoint).Address);

                if (bytesRead == 0)
                {
                    // Something goes wrong, close connection
                    CloseConnection(state);
                }
                else
                {
                    // TODO: remove first data received if it starts with cumbersome characters
                    string dataReceived = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

                    // If data ends with CRLF, remove them and consider command as complete
                    bool commandComplete = false;
                    if (dataReceived.EndsWith("\n") || dataReceived.EndsWith("\r"))
                    {
                        commandComplete = true;
                        dataReceived = dataReceived.TrimEnd('\r', '\n');
                    }

                    // Append data in command
                    state.Command.Append(dataReceived);

                    // Command is complete, send it to command processor and start a new one
                    if (commandComplete)
                    {
                        // Get command
                        string command = state.Command.ToString();
                        Log.Default.WriteLine(LogLevels.Info, "Command received from client at " + ((IPEndPoint) clientSocket.RemoteEndPoint).Address + " : " + command);

                        // Reset command
                        state.Command = new StringBuilder();

                        // Test mode: send command back to client
                        Send(clientSocket, command);

                        // Process command
                        state.Client.ProcessCommand(command);
                    }

                    // Continue reading
                    clientSocket.BeginReceive(state.Buffer, 0, ClientSocketStateObject.BufferSize, 0, ReadCallback, state);
                }
            }
            catch (ObjectDisposedException)
            {
                // If server status is stopping/stopped: ok
                // else, throw
                if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped)
                    throw;
            }
        }

        private void Send(System.Net.Sockets.Socket clientSocket, string data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                Log.Default.WriteLine(LogLevels.Debug, "Send data to client at " + ((IPEndPoint) clientSocket.RemoteEndPoint).Address + " : " + data);

                // Begin sending the data to the remote device.
                clientSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, clientSocket);
            }
            catch (ObjectDisposedException)
            {
                // If server status is stopping/stopped: ok
                // else, throw
                if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped)
                    throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                System.Net.Sockets.Socket clientSocket = (System.Net.Sockets.Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = clientSocket.EndSend(ar);

                Log.Default.WriteLine(LogLevels.Debug, "Sent " + bytesSent + " bytes to client at " + ((IPEndPoint) clientSocket.RemoteEndPoint).Address);
            }
            catch (ObjectDisposedException)
            {
                // If server status is stopping/stopped: ok
                // else, throw
                if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped)
                    throw;
            }
        }

        private void CloseConnection(ClientSocketStateObject state)
        {
            // Remove from 'client' collection
            _serverStateObjects.Remove(state);

            //
            System.Net.Sockets.Socket clientSocket = state.ClientSocket;
            IPlayer client = state.Client;

            Log.Default.WriteLine(LogLevels.Info, "Client at " + ((IPEndPoint)clientSocket.RemoteEndPoint).Address + " has disconnected");
            
            // Close socket
            clientSocket.Shutdown(SocketShutdown.Both);
            client.OnDisconnected();
        }

        public void Dispose()
        {
            _listenTask = null;

            _listenEvent.Dispose();
            _listenEvent.Close();
            _listenEvent = null;

            _serverSocket.Shutdown(SocketShutdown.Both);
            _serverSocket.Close();
            _serverSocket = null;
        }
    }
}
