using System.Collections.Generic;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IAbilityManager
    {
        IAbility WeakenedSoulAbility { get; }
        IAbility ParryAbility { get; }
        IAbility DodgerAbility { get; }
        IAbility ShieldBlockAbility { get; }

        IEnumerable<IAbility> Abilities { get; }
        
        IAbility this[int id] { get; }
        IAbility this[string name] { get; }

        IAbility Search(CommandParameter parameter, bool includePassive = false);

        bool Process(ICharacter source, params CommandParameter[] parameters);

        // TEST: TO REMOVE
        bool Process(ICharacter source, ICharacter target, IAbility ability);
    }
}
