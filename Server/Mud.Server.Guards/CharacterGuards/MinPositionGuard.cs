using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.CharacterGuards;

public class MinPositionGuard(Positions minPosition) : ICharacterGuard
{
    public Positions MinPosition { get; } = minPosition;

    public string? Guards(ICharacter actor, IActionInput actionInput, IGameAction gameAction)
       => Guards(actor);

    public string? Guards(ICharacter actor, ICommandParameter[] commandParameters)
        => Guards(actor);

    private string? Guards(ICharacter actor)
    {
        if (actor.Position < MinPosition)
            return actor.Position switch
            {
                Positions.Sleeping => "In your dreams, or what?",
                Positions.Resting => "Nah... You feel too relaxed...",
                Positions.Sitting => "Better stand up first.",
                _ => null,
            };
        return null;
    }
}