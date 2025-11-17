using Mud.Server.Common;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin
{
    public class AdminManager : IAdminManager
    {
        private readonly List<IAdmin> _admins;

        public AdminManager()
        {
            _admins = [];
        }

        public void AddAdmin(IAdmin admin)
        {
            if (!_admins.Contains(admin))
                _admins.Add(admin);
        }

        public void RemoveAdmin(IAdmin admin)
        {
            _admins.Remove(admin);
        }

        public IAdmin? GetAdmin(ICommandParameter parameter, bool perfectMatch)
            => FindHelpers.FindByName(_admins, parameter, perfectMatch);

        public IEnumerable<IAdmin> Admins
            => _admins;
    }
}
