using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityManager
    {
        IEnumerable<IAbilityDefinition> Abilities { get; }
        IAbilityDefinition? this[string abilityName] { get; }
        IAbilityDefinition? this[Type abilityType] { get; }
        IAbilityDefinition? this[WeaponTypes weaponType] { get; }

        IEnumerable<IAbilityDefinition> SearchAbilities<TAbility>()
            where TAbility: class, IAbility;
        IEnumerable<IAbilityDefinition> SearchAbilitiesByExecutionType<TAbility>()
            where TAbility : class, IAbility;

        IAbilityDefinition? Search(string pattern, AbilityTypes type);
        IAbilityDefinition? Search(ICommandParameter parameter);

        TAbility? CreateInstance<TAbility>(string abilityName)
            where TAbility : class, IAbility;
        TAbility? CreateInstance<TAbility>(IAbilityDefinition abilityDefinition)
            where TAbility : class, IAbility;
    }
}
