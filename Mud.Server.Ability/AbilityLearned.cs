using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability;

public class AbilityLearned : IAbilityLearned
{
    public string Name { get; }

    public int Level { get; protected set; }

    public ResourceKinds? ResourceKind { get; }

    public int CostAmount { get; protected set; }

    public CostAmountOperators CostAmountOperator { get; }

    public int Rating { get; protected set; }

    public IAbilityDefinition AbilityDefinition { get; }

    public int Learned { get; protected set; }

    public AbilityLearned(IAbilityUsage abilityUsage)
    {
        Name = abilityUsage.Name;
        Level = abilityUsage.Level;
        ResourceKind = abilityUsage.ResourceKind;
        CostAmount = abilityUsage.CostAmount;
        CostAmountOperator = abilityUsage.CostAmountOperator;
        Rating = abilityUsage.Rating;
        AbilityDefinition = abilityUsage.AbilityDefinition;
        Learned = 0; // can be gained but not yet learned
    }

    public AbilityLearned(LearnedAbilityData learnedAbilityData, IAbilityDefinition abilityDefinition)
    {
        Name = learnedAbilityData.Name;
        Level = learnedAbilityData.Level;
        ResourceKind = learnedAbilityData.ResourceKind;
        CostAmount = learnedAbilityData.CostAmount;
        CostAmountOperator = learnedAbilityData.CostAmountOperator;
        Rating = learnedAbilityData.Rating;
        AbilityDefinition = abilityDefinition;
        Learned = learnedAbilityData.Learned;
    }

    public void IncrementLearned(int amount)
    {
        Learned = Math.Min(100, Learned + amount);
    }

    public void SetLearned(int amount)
    {
        Learned = Math.Clamp(amount, 0, 100);
    }

    public LearnedAbilityData MapLearnedAbilityData()
    {
        return new LearnedAbilityData
        {
            Name = Name,
            ResourceKind = ResourceKind,
            CostAmount = CostAmount,
            CostAmountOperator = CostAmountOperator,
            Level = Level,
            Learned = Learned,
            Rating = Rating,
        };
    }

    public void Update(int level, int rating, int costAmount, int learned)
    {
        Level = level;
        Rating = rating;
        CostAmount = costAmount;
        Learned = learned;
    }

    public void Update(int level, int rating, int learned)
    {
        Level = level;
        Rating = rating;
        Learned = learned;
    }

    public bool CanBePracticed(IPlayableCharacter playableCharacter)
        => Level <= playableCharacter.Level && Learned > 0;

    public bool HasCost()
    {
        if (!ResourceKind.HasValue)
            return false;
        return CostAmountOperator switch
        {
            CostAmountOperators.None => false,
            CostAmountOperators.All => true,
            _ => CostAmount > 0
        };
    }
}
