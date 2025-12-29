namespace Mud.Server.Interfaces.Ability;

public interface IAbilityUsage
{
    string Name { get; }

    int Level { get; }

    IAbilityResourceCost[] ResourceCosts { get; }

    int Rating { get; }

    int MinLearned { get; }

    IAbilityDefinition AbilityDefinition { get; }

    bool HasCost { get; }
}
