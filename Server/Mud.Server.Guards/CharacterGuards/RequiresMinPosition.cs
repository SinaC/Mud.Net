using Mud.Domain;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.CharacterGuards;

public class RequiresMinPosition(Positions minPosition) : IGuard<ICharacter>
{
    public Positions MinPosition { get; } = minPosition;

    public string? Guards(ICharacter character, IActionInput actionInput, IGameAction gameAction)
    {
        if (character.Position < MinPosition)
            return character.Position switch
            {
                Positions.Sleeping => "In your dreams, or what?",
                Positions.Resting => "Nah... You feel too relaxed...",
                Positions.Sitting => "Better stand up first.",
                _ => null,
            };
        return null;
    }
}