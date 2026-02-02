using Mud.Domain;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain;

namespace Mud.Server.Ability.Interfaces;

public interface IAbilityManager
{
    IEnumerable<IAbilityDefinition> Abilities { get; }
    IAbilityDefinition? this[string abilityName] { get; }
    IAbilityDefinition? this[Type abilityType] { get; }
    IAbilityDefinition? this[WeaponTypes weaponType] { get; }

    IEnumerable<IAbilityDefinition> SearchAbilitiesByExecutionType<TAbility>()
        where TAbility : class, IAbility;

    IAbilityDefinition? Get(string abilityName, AbilityTypes type);

    IEnumerable<IAbilityDefinition> Search(ICommandParameter parameter, AbilityTypes type);

    TAbility? CreateInstance<TAbility>(string abilityName)
        where TAbility : class, IAbility;
    TAbility? CreateInstance<TAbility>(IAbilityDefinition abilityDefinition)
        where TAbility : class, IAbility;
}
