using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class MustBeImpersonatedGuard : IPlayerGuard
{
    public string? Guards(IPlayer actor)
    {
        if (actor.Impersonating == null)
            return $"You must be impersonated to use that.";
        return null;
    }
}