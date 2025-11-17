using Mud.Network.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    /*
    public class AdminManagerMock : IAdminManager
    {
        private readonly List<IAdmin> _admins = new List<IAdmin>();

        public IEnumerable<IAdmin> Admins => _admins;

        public IAdmin AddAdmin(IClient client, string name)
        {
            IAdmin admin = new Admin.Admin(Guid.NewGuid(), name);
            _admins.Add(admin);
            return admin;
        }

        public IAdmin GetAdmin(ICommandParameter parameter, bool perfectMatch)
        {
            throw new NotImplementedException();
        }
    }
    */
}
