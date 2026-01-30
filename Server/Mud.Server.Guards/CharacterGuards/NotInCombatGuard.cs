using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class NotInCombatGuard(string? message) : ICharacterGuard
{
    public string? Message => message;

    public string? Guards(ICharacter actor, IActionInput actionInput, IGameAction gameAction)
       => Guards(actor);

    public string? Guards(ICharacter actor, ICommandParameter[] commandParameters)
        => Guards(actor);

    private string? Guards(ICharacter actor)
    {
        if (actor.Fighting != null)
            return Message ?? "No way!  You are still fighting!";
        return null;
    }
}