using System.Collections;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public interface IAbilityManager
    {
        IEnumerable<AbilityInfo> Abilities { get; }
        AbilityInfo this[string abilityName] { get; }
    }
}
