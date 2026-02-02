using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.CharacterGuards;

public class CannotBeInCombat : IGuard<ICharacter>
{
    public string? Message { get; init; }

    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
    {
        if (character.Fighting != null)
            return Message ?? "No way! You are still fighting!";
        return null;
    }
}