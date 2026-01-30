using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Guards.PlayerGuards;

public class CannotBeImpersonatedGuard : IPlayerGuard
{
    public string? Guards(IPlayer actor, IActionInput actionInput, IGameAction gameAction)
       => Guards(actor);

    public string? Guards(IPlayer actor, ICommandParameter[] commandParameters)
        => Guards(actor);

    private string? Guards(IPlayer actor)
    {
        if (actor.Impersonating != null)
            return $"You cannot be impersonated to use that.";
        return null;
    }
}