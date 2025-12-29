using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Ability;

public class AbilityUsage : IAbilityUsage
{
    public string Name { get; }

    public int Level { get; }

    public IAbilityResourceCost[] ResourceCosts { get; }

    public int Rating { get; }

    public int MinLearned { get; }

    public IAbilityDefinition AbilityDefinition { get; }

    public bool HasCost => ResourceCosts.Length > 0;

    public AbilityUsage(string name, int level, IEnumerable<IAbilityResourceCost> resourceCosts, int rating, int minLearned, IAbilityDefinition abilityDefinition)
    {
        Name = name;
        Level = level;
        ResourceCosts = resourceCosts.ToArray();
        Rating = rating;
        MinLearned = minLearned;
        AbilityDefinition = abilityDefinition;
    }
}
