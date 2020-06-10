using Mud.POC.Abilities2.ExistingCode;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public interface ISpell : IAbility
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Setup(SpellActionInput spellActionInput);
        // Execute the action, Guards must be called before
        void Execute();
        IEnumerable<IEntity> AvailableTargets(ICharacter caster);
    }
}
