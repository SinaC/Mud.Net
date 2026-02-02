using Mud.Server.Ability.Interfaces;

namespace Mud.Server.AbilityGroup.Interfaces;

public interface IAbilityGroupDefinition
{
    string Name { get; }
    string? Help { get; }
    string? OneLineHelp { get; }

    IEnumerable<IAbilityGroupDefinition> AbilityGroupDefinitions { get; }
    IEnumerable<IAbilityDefinition> AbilityDefinitions { get; }
}
