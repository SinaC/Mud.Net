using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Guards.Interfaces;

public interface ISpellGuard
{
    string? Guards(ICharacter character, ISpellActionInput spellActionInput);
}