using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class CannotBeInCombat : CharacterGuards.CannotBeInCombat, IGuard<IPlayer>
{
    public string? Guards(IPlayer player, IActionInput actionInput, IGameAction gameAction)
    {
        if (player.Impersonating != null)
            Guards(player.Impersonating, actionInput, gameAction);
        return null;
    }
}
