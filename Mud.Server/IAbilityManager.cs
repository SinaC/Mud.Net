using System.Collections.Generic;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IAbilityManager
    {
        IReadOnlyCollection<IAbility> Abilities { get; }
        
        IAbility this[int id] { get; }
        IAbility this[string name] { get; }

        IAbility Search(CommandParameter parameter);

        bool Process(ICharacter source, params CommandParameter[] parameters);

        // TEST: TO REMOVE
        bool Process(ICharacter source, ICharacter target, IAbility ability);
    }
}
