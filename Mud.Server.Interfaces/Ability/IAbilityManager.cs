using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityManager
    {
        IEnumerable<IAbilityInfo> Abilities { get; }
        IAbilityInfo this[string abilityName] { get; }

        IEnumerable<IAbilityInfo> AbilitiesByExecutionType<TAbility>()
            where TAbility : class, IAbility;

        IAbilityInfo Search(string pattern, AbilityTypes type);
        IAbilityInfo Search(ICommandParameter parameter);

        TAbility CreateInstance<TAbility>(string abilityName)
            where TAbility : class, IAbility;
        TAbility CreateInstance<TAbility>(IAbilityInfo abilityInfo)
            where TAbility : class, IAbility;
    }
}
