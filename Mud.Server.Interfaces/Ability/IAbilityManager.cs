using System.Collections.Generic;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityManager
    {
        IEnumerable<IAbilityInfo> Abilities { get; }
        IAbilityInfo this[string abilityName] { get; }

        IAbilityInfo Search(string pattern, AbilityTypes type);

        TAbility CreateInstance<TAbility>(string abilityName)
            where TAbility : class, IAbility;
    }
}
