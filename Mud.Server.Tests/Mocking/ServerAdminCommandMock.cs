using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;
using System;

namespace Mud.Server.Tests.Mocking
{
    public class ServerAdminCommandMock : IServerAdminCommand
    {
        public void Promote(IPlayer player, AdminLevels level)
        {
            throw new NotImplementedException();
        }

        public void Shutdown(int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
