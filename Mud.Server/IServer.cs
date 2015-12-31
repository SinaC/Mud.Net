using Mud.Network;

namespace Mud.Server
{
    public interface IServer
    {
        bool IsAsynchronous { get; }

        void Initialize(bool asynchronous, INetworkServer networkServer);
        void Start();
        void Stop();

        void Shutdown(int seconds);

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddPlayer(IClient client, string name);
        IAdmin AddAdmin(IClient client, string name);
    }
}
