using Mud.Domain;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Ability;

public class AbilityUsage : IAbilityUsage
{
    public string Name { get; }

    public int Level { get; protected set; }

    public ResourceKinds? ResourceKind { get; }

    public int CostAmount { get; protected set; }

    public CostAmountOperators CostAmountOperator { get; }

    public int Rating { get; protected set; }

    public IAbilityInfo AbilityInfo { get; }

    public AbilityUsage(string name, int level, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, IAbilityInfo abilityInfo)
    {
        Name = name;
        Level = level;
        ResourceKind = resourceKind;
        CostAmount = costAmount;
        CostAmountOperator = costAmountOperator;
        Rating = rating;
        AbilityInfo = abilityInfo;
    }

    public void Update(int level, int rating, int costAmount)
    {
        Update(level, rating);
        CostAmount = costAmount;
    }

    public void Update(int level, int rating)
    {
        Level = level;
        Rating = rating;
    }
}
