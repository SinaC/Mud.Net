using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Guards;

public interface ISpellGuard
{
    string? Guards(ICharacter character, ISpellActionInput spellActionInput);
}