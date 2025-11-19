namespace Mud.Network.Interfaces;

public interface ITelnetNetworkServer : INetworkServer
{
    void SetPort(int port);
}
