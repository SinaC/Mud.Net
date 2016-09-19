using System;
using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IServer
    {
        DateTime CurrentTime { get; } // Centralized time synchronized on server's pulse

        void Initialize(List<INetworkServer> networkServers);
        void Start();
        void Stop();

        void Shutdown(int seconds);
        void Quit(IPlayer player);

        void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

        IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch);
        IEnumerable<IPlayer> Players { get; }
        IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch);
        IEnumerable<IAdmin> Admins { get; }

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddPlayer(IClient client, string name);
        IAdmin AddAdmin(IClient client, string name);
    }
}
