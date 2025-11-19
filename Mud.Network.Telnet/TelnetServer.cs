using Microsoft.Extensions.Logging;
using Mud.Network.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Mud.Network.Telnet;

// TODO: handling exception
// TODO: forbidding start/stop/... depending on server state

internal enum ServerStatus
{
    Creating,
    Created,
    PortInitialized,
    Initializing,
    Initialized,
    Starting,
    Started,
    Stopping,
    Stopped
}

// TODO: problem with simple telnet terminal without handshake
public class TelnetServer : ITelnetNetworkServer, IDisposable
{
    private static readonly Regex AsciiRegEx = new Regex(@"[^\u0020-\u007E]", RegexOptions.Compiled); // match ascii-only char

    private ILogger<TelnetServer> Logger { get; }

    private Socket _serverSocket = default!;
    private ManualResetEvent _listenEvent = default!;
    private Task _listenTask = default!;
    private CancellationTokenSource _cancellationTokenSource = default!;

    private ServerStatus _status;
    private List<ClientTelnetStateObject> _clients { get; } = default!;

    public int Port { get; private set; }

    public TelnetServer(ILogger<TelnetServer> logger)
    {
        Logger = logger;

        _status = ServerStatus.Creating;
        Logger.LogInformation("Socket server creating");

        _clients = [];

        Logger.LogInformation("Socket server created");
        _status = ServerStatus.Created;
    }

    #region INetworkServer

    public event NewClientConnectedEventHandler? NewClientConnected;
    public event ClientDisconnectedEventHandler? ClientDisconnected;

    public void SetPort(int port)
    {
        Port = port;

        if (_status != ServerStatus.Created)
        {
            Logger.LogError("Socket server cannot set port. Status: {status}", _status);
            return;
        }

        _status = ServerStatus.PortInitialized;

    }

    public void Initialize()
    {
        if (_status != ServerStatus.PortInitialized)
        {
            Logger.LogError("Socket server cannot be initialized. Status: {status}", _status);
            return;
        }

        Logger.LogInformation("Socket server initializing");

        _status = ServerStatus.Initializing;

        // Establish the local endpoint for the socket.
        string hostName = Dns.GetHostName();
        IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new(ipAddress, Port);

        // Create a TCP/IP socket.
        _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Listen task event
        _listenEvent = new ManualResetEvent(false);

        // Bind the socket to the local endpoint and listen for incoming connections.
        _serverSocket.Bind(localEndPoint);
        Logger.LogInformation("Socket server bound to {hostName} on port {port}", hostName, localEndPoint.Port);

        _status = ServerStatus.Initialized;
        Logger.LogInformation("Socket server initialized");
    }

    public void Start()
    {
        if (_status != ServerStatus.Initialized)
        {
            Logger.LogError("Socket server cannot be started. Status: {status}", _status);
            return;
        }

        Logger.LogInformation("Socket server starting");

        _status = ServerStatus.Starting;

        _serverSocket.Listen(100);
        _cancellationTokenSource = new CancellationTokenSource();
        _listenTask = Task.Factory.StartNew(ListenTask, _cancellationTokenSource.Token);

        _status = ServerStatus.Started;

        Logger.LogInformation("Socket server started");
    }

    public void Stop()
    {
        Logger.LogInformation("Socket server stopping");

        _status = ServerStatus.Stopping;

        Broadcast("STOPPING SERVER NOW!!!");

        try
        {
            // Close clients socket
            ClientTelnetStateObject[] clone = _clients.Select(x => x).ToArray(); // make a copy, because CloseConnection modify collection
            foreach (ClientTelnetStateObject client in clone)
                CloseConnection(client);
            _clients.Clear();
            
            // Stop listen task
            _cancellationTokenSource.Cancel();
            _listenEvent.Set(); // wakeup thread
            _listenTask.Wait(2000);

            // Close server socket
            //_serverSocket.Shutdown(SocketShutdown.Both); // TODO: exception on this line when closing application
            _serverSocket.Close();
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning("Operation canceled exception while stopping. Exception: {ex}", ex);
        }
        catch (AggregateException ex)
        {
            Logger.LogWarning("Aggregate exception while stopping. Exception: {ex}", ex.Flatten());
        }

        Logger.LogInformation("Socket server stopped");

        _status = ServerStatus.Stopped;
    }

    public void Broadcast(string data)
    {
        foreach(ClientTelnetStateObject client in _clients)
            SendData(client, data);
    }

    #endregion

    private void ListenTask()
    {
        try
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.LogInformation("Stop ListenTask requested");
                    break;
                }

                // Set the event to nonsignaled state.
                _listenEvent.Reset();

                Logger.LogDebug("Waiting for connection");

                // Start an asynchronous socket to listen for connections.
                _serverSocket.BeginAccept(AcceptCallback, _serverSocket);

                // Wait until a connection is made before continuing.
                _listenEvent.WaitOne();
            }
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError("ListenTask exception. Exception: {ex}", ex);
        }

        Logger.LogInformation("ListenTask stopped");
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            // Signal the ListenTask to continue.
            _listenEvent.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);

            Logger.LogDebug("Client connected from {address}", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???");

            // Create the state object.
            ClientTelnetStateObject client = new (this)
            {
                ClientSocket = clientSocket,
            };
            _clients.Add(client);
            // TODO: NewClientConnected will be called once protocol handshake is done
            //
            client.State = ClientStates.Connected;
            NewClientConnected?.Invoke(client);
            //
            clientSocket.BeginReceive(client.Buffer, 0, ClientTelnetStateObject.BufferSize, 0, ReadCallback, client);
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
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        ClientTelnetStateObject client = (ClientTelnetStateObject)ar.AsyncState;
        try
        {
            Socket clientSocket = client.ClientSocket;

            // Read data from the client socket. 
            int bytesRead = clientSocket.EndReceive(ar);

            Logger.LogDebug("Received {count} bytes from {address}", bytesRead, (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???");

            if (bytesRead == 0)
            {
                // Something goes wrong, close connection
                CloseConnection(client); // this will cause a recursive call because ClientDisconnected call cause CloseConnection
            }
            else
            {
                //    // Putty handshake is 
                //    //FF FB 1F window size
                //    //FF FB 20 terminal speed
                //    //FF FB 18 terminal type
                //    //FF FB 27 Telnet Environment Option
                //    //FF FD 01 echo
                //    //FF FB 03 suppress go ahead
                //    //FF FD 03 suppress go ahead

                //    //FF FE 01 received after echo on

                //Logger.LogDebug("Data received from client at " + ((IPEndPoint)clientSocket.RemoteEndPoint).Address + " : " + ByteArrayToString(client.Buffer, bytesRead));

                // Remove protocol character
                if (bytesRead % 3 == 0 && client.Buffer[0] == 0xFF)// && client.Buffer[1] == 0xFB)
                {
                    Logger.LogInformation("Protocol received from {address} : {protocol}", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???", ByteArrayToString(client.Buffer, bytesRead));
                }
                else if (client.State == ClientStates.Connected)
                {
                    string dataReceived = Encoding.ASCII.GetString(client.Buffer, 0, bytesRead);

                    Logger.LogDebug("Data received from {address} : {data} ", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???", dataReceived);

                    // If data ends with CRLF, remove CRLF and consider command as complete
                    bool commandComplete = false;
                    if (dataReceived.EndsWith('\n') || dataReceived.EndsWith('\r'))
                    {
                        commandComplete = true;
                        dataReceived = dataReceived.TrimEnd('\r', '\n');
                    }

                    // Remove non-ascii char
                    dataReceived = AsciiRegEx.Replace(dataReceived, string.Empty);

                    // Append data in command
                    client.Command.Append(dataReceived);

                    // Command is complete, send it to command processor and start a new one
                    if (commandComplete)
                    {
                        // Get command
                        string command = client.Command.ToString();
                        Logger.LogInformation("Command received from {address} : {command} ", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???", command);

                        // Reset command
                        client.Command.Clear();

                        // Process command
                        client.OnDataReceived(command);
                    }
                }
                // TODO: other states ?

                // Continue reading
                clientSocket.BeginReceive(client.Buffer, 0, ClientTelnetStateObject.BufferSize, 0, ReadCallback, client);
            }
        }
        catch (ObjectDisposedException)
        {
            // If server status is stopping/stopped: ok
            // else, throw
            if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped && client.State != ClientStates.Disconnected)
                throw;
        }
    }

    internal void SendData(ClientTelnetStateObject client, string data)
    {
        try
        {
            Socket clientSocket = client.ClientSocket;

            Logger.LogDebug("Send data to {address} : {data} ", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???", data);

            // Colorize or strip color
            string colorizedData = client.ColorAccepted 
                ? AnsiHelpers.Colorize(data) 
                : AnsiHelpers.StripColor(data);

            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(colorizedData);

            // Begin sending the data to the remote device.
            clientSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }
        catch (ObjectDisposedException)
        {
            // If server status is stopping/stopped: ok
            // else, throw
            if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped && client.State != ClientStates.Disconnected)
                throw;
        }
    }

    internal void SendData(ClientTelnetStateObject client, byte[] byteData)
    {
        try
        {
            Socket clientSocket = client.ClientSocket;

            Logger.LogDebug("Send data to {adress} : {data}", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???", ByteArrayToString(byteData, byteData.Length));

            // Begin sending the data to the remote device.
            clientSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }
        catch (ObjectDisposedException)
        {
            // If server status is stopping/stopped: ok
            // else, throw
            if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped && client.State != ClientStates.Disconnected)
                throw;
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            ClientTelnetStateObject client = (ClientTelnetStateObject)ar.AsyncState;
            Socket clientSocket = client.ClientSocket;

            // Complete sending the data to the remote device.
            int bytesSent = clientSocket.EndSend(ar);

            Logger.LogDebug("Sent {count} bytes to {address}", bytesSent, (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???");
        }
        catch (ObjectDisposedException)
        {
            // If server status is stopping/stopped: ok
            // else, throw
            if (_status != ServerStatus.Stopping && _status != ServerStatus.Stopped)
                throw;
        }
    }

    internal void CloseConnection(ClientTelnetStateObject client)
    {
        // Remove from 'client' collection
        _clients.Remove(client);

        //
        Socket clientSocket = client.ClientSocket;

        // Only if not connected
        if (clientSocket.Connected)
        {
            Logger.LogInformation("Client {address} has disconnected", (clientSocket.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "???");

            // Close socket
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            ClientDisconnected?.Invoke(client); // this will cause a recursive call
        }
    }

    private static string ByteArrayToString(byte[] ba, int length)
    {
        StringBuilder hex = new StringBuilder(length * 3);
        for(int i = 0; i < length; i++)
            hex.Append($"{ba[i]:X2} ");
        return hex.ToString();
    }

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeOfManagedResourcesInAdditionToUnmanagedResources)
    {
        if (disposeOfManagedResourcesInAdditionToUnmanagedResources)
        {
            if (_listenTask != null)
            {
                _listenTask.Dispose();
                _listenTask = null;
            }

            if (_listenEvent != null)
            {
                _listenEvent.Dispose();
                _listenEvent.Close();
                _listenEvent = null;
            }

            if (_serverSocket != null)
            {
                _serverSocket.Shutdown(SocketShutdown.Both);
                _serverSocket.Close();
                _serverSocket = null;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }

    #endregion
}
