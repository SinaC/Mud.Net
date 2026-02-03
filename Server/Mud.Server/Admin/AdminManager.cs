using Mud.Common.Attributes;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Admin;

namespace Mud.Server.Admin;

[Export(typeof(IAdminManager)), Shared]
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
