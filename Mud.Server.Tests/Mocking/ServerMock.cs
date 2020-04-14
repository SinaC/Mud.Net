using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Tests.Mocking
{
    // TODO

    public class ServerMock : IServer
    {
        public DateTime CurrentTime { get; private set; }

        public void Initialize(List<INetworkServer> networkServers)
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

        public void Quit(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void Delete(IPlayer player)
        {
            throw new NotImplementedException(); 
        }

        public void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            throw new NotImplementedException();
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlayer> Players
        {
            get { throw new NotImplementedException(); }
        }

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAdmin> Admins
        {
            get { throw new NotImplementedException(); }
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
