using Mud.Network.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace Mud.Network.Telnet;

internal enum ClientStates
{
    Handshaking,
    Connected,
    Disconnected
}

// State object for reading client data asynchronously
internal class ClientTelnetStateObject : IClient
{
    private static readonly byte[] EchoOffData = [0xFF, 0xFB, 0x01];
    private static readonly byte[] EchoOnData = [0xFF, 0xFC, 0x01];

    // Size of receive buffer.
    public const int BufferSize = 256;
    // Client socket.
    public Socket ClientSocket { get; set; }
    // Receive buffer.
    public byte[] Buffer { get; set; }
    // Received data string.
    public StringBuilder Command { get; set; }
    // Server
    public TelnetServer Server { get; }
    // State
    public ClientStates State { get; set; }

    public ClientTelnetStateObject(TelnetServer server)
    {
        DataReceived = default!;
        Server = server;
        Buffer = new byte[BufferSize];
        Command = new StringBuilder();
        ClientSocket = default!;
        ColorAccepted = true; // by default
        State = ClientStates.Handshaking;
    }

    public void OnDataReceived(string data)
    {
        DataReceived?.Invoke(this, data);
    }

    #region IClient

    public event DataReceivedEventHandler DataReceived;

    public bool IsConnected => State == ClientStates.Connected;

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
        if (State != ClientStates.Disconnected)
        {
            State = ClientStates.Disconnected;
            Server.CloseConnection(this);
        }
    }

    #endregion
}
