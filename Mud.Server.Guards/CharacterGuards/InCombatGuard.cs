using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class InCombatGuard(string? message) : ICharacterGuard
{
    public string? Message => message;

    public string? Guards(ICharacter actor)
    {
        if (actor.Fighting == null)
            return Message ?? "You aren't fighting anyone.";
        return null;
    }
}