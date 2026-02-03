using Mud.Server.Ability.Interfaces;

namespace Mud.Server.Ability.Spell.Interfaces;

public interface ISpell : IAbility
{
    // Guards the action against incorrect usage
    // Returns null if all guard pass
    // Returns error message describing failure
    string? Setup(ISpellActionInput spellActionInput);
    // Execute the action, Guards must be called before
    void Execute();
}
