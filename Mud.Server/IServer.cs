using Mud.Network;

namespace Mud.Server
{
    public interface IServer
    {
        void Initialize(INetworkServer networkServer);
        void Start();
        void Stop();

        void Shutdown(int seconds);

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddClient(IClient client, string name);
        IAdmin AddAdmin(IClient client, string name);
    }
}
