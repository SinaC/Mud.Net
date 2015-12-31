using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IServer
    {
        bool IsAsynchronous { get; }

        void Initialize(INetworkServer networkServer, bool asynchronous);
        void Start();
        void Stop();

        void Shutdown(int seconds);

        IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch);
        IReadOnlyCollection<IPlayer> GetPlayers();
        IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch);
        IReadOnlyCollection<IAdmin> GetAdmins();

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddPlayer(IClient client, string name);
        IAdmin AddAdmin(IClient client, string name);
    }
}
