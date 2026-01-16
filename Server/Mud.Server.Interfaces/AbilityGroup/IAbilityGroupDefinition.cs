using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupDefinition
    {
        string Name { get; }
        string? Help { get; }
        string? OneLineHelp { get; }

        IEnumerable<IAbilityGroupDefinition> AbilityGroupDefinitions { get; }
        IEnumerable<IAbilityDefinition> AbilityDefinitions { get; }
    }
}
