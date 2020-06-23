using Mud.Network;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    public class AdminManagerMock : IAdminManager
    {
        public IEnumerable<IAdmin> Admins => throw new NotImplementedException();

        public IAdmin AddAdmin(IClient client, string name)
        {
            throw new NotImplementedException();
        }

        public IAdmin GetAdmin(ICommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }
    }
}
