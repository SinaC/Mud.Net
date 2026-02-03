using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class MustBeImpersonated : IGuard<IPlayer>
{
    public string? Guards(IPlayer player, IActionInput actionInput, IGameAction gameAction)
    {
        if (player.Impersonating == null)
            return $"You must be impersonated to use that.";
        return null;
    }
}