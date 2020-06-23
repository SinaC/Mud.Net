using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;
using System;

namespace Mud.Server.Tests.Mocking
{
    public class ServerPlayerCommandMock : IServerPlayerCommand
    {
        public void Delete(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void Quit(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void Save(IPlayer player)
        {
            throw new NotImplementedException();
        }
    }
}
