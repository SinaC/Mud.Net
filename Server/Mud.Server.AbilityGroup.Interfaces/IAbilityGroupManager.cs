using Mud.Server.CommandParser.Interfaces;

namespace Mud.Server.AbilityGroup.Interfaces;

public interface IAbilityGroupManager
{
    IEnumerable<IAbilityGroupDefinition> AbilityGroups { get; }

    IAbilityGroupDefinition? this[string abilityGroupName] { get; }

    IAbilityGroupDefinition? Search(ICommandParameter parameter);
}
