using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class RequiresAtLeastOneArgument : ActorGuards.RequiresAtLeastOneArgument, IGuard<IPlayer>
{
    public string? Guards(IPlayer player, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(player, actionInput, gameAction);
}
