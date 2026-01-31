using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class CannotBeImpersonated : IGuard<IPlayer>
{
    public string? Guards(IPlayer player, IActionInput actionInput, IGameAction gameAction)
    {
        if (player.Impersonating != null)
            return $"You cannot be impersonated to use that.";
        return null;
    }
}