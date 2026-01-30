using Mud.Domain;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.AdminGuards;

public class MinAdminLevelGuard(AdminLevels minAdminLevel) : IAdminGuard
{
    public AdminLevels MinAdminLevel { get; } = minAdminLevel;

    public string? Guards(IAdmin actor, IActionInput actionInput, IGameAction gameAction)
        => Guards(actor);

    public string? Guards(IAdmin actor, ICommandParameter[] commandParameters)
        => Guards(actor);

    private string? Guards(IAdmin actor)
    {
        if (actor.Level < MinAdminLevel)
            return "Command not found";
        return null;
    }
}