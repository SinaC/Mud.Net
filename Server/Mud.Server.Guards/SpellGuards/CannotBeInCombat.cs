using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

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