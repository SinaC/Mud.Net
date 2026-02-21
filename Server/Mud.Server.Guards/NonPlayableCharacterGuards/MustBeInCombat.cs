using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.NonPlayableCharacterGuards;

public class MustBeInCombat : IGuard<INonPlayableCharacter>
{
    public string? Message { get; init; }

    public string? Guards(INonPlayableCharacter character, IActionInput actionInput, IGameAction gameAction)
    {
        if (character.Fighting == null)
            return Message ?? "You aren't fighting anyone.";
        return null;
    }
}