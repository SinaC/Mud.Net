using System;
using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Tests.Mocking
{
    // TODO

    public class ServerMock : IServer
    {
        public bool IsAsynchronous { get; private set; }
        public void Initialize(INetworkServer networkServer, bool asynchronous)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Shutdown(int seconds)
        {
            throw new NotImplementedException();
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            throw new NotImplementedException();
        }

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<IAdmin> GetAdmins()
        {
            throw new NotImplementedException();
        }

        public IPlayer AddPlayer(IClient client, string name)
        {
            throw new NotImplementedException();
        }

        public IAdmin AddAdmin(IClient client, string name)
        {
            throw new NotImplementedException();
        }
    }
}
