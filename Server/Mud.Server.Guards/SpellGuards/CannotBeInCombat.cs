using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Guards.SpellGuards;

public class CannotBeInCombat : ISpellGuard
{
    public string? Message { get; init; }

    public string? Guards(ICharacter actor, ISpellActionInput spellActionInput)
    {
        if (actor.Fighting != null)
            return Message ?? StringHelpers.YouCantConcentrateEnough;
        return null;
    }
}