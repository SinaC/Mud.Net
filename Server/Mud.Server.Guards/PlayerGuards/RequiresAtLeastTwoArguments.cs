using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class RequiresAtLeastTwoArguments : ActorGuards.RequiresAtLeastTwoArguments, IGuard<IPlayer>
{
    public string? Guards(IPlayer player, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(player, actionInput, gameAction);
}
