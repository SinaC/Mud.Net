using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupManager
    {
        IEnumerable<IAbilityGroupDefinition> AbilityGroups { get; }

        IAbilityGroupDefinition? this[string abilityGroupName] { get; }

        IAbilityGroupDefinition? Search(ICommandParameter parameter);
    }
}
