using Mud.Domain;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Guards.SpellGuards;

public class RequiresMinPosition(Positions minPosition) : ISpellGuard
{
    public Positions MinPosition { get; } = minPosition;

    public string? Guards(ICharacter actor, ISpellActionInput spellActionInput)
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