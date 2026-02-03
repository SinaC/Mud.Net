namespace Mud.Server.Ability.Interfaces;

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
