using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability;

public class AbilityLearned : AbilityUsage, IAbilityLearned
{
    public int Learned { get; protected set; }

    public AbilityLearned(IAbilityUsage abilityUsage)
        : base(abilityUsage.Name, abilityUsage.Level, abilityUsage.ResourceKind, abilityUsage.CostAmount, abilityUsage.CostAmountOperator, abilityUsage.Rating, abilityUsage.AbilityInfo)
    {
        Learned = 0; // can be gained but not yet learned
    }

    public AbilityLearned(LearnedAbilityData learnedAbilityData, IAbilityInfo abilityInfo)
        : base(learnedAbilityData.Name, learnedAbilityData.Level, learnedAbilityData.ResourceKind, learnedAbilityData.CostAmount, learnedAbilityData.CostAmountOperator, learnedAbilityData.Rating, abilityInfo)
    {
        Learned = learnedAbilityData.Learned;
    }

    public void IncrementLearned(int amount)
    {
        if (amount <= 0)
        {
            Log.Default.WriteLine(LogLevels.Error, "Trying to decrement learned of ability {0} by {1}.", Name, amount);
            return;
        }
        Learned = Math.Min(100, Learned + amount);
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

    public bool CanBeGained(IPlayableCharacter playableCharacter) => Level <= playableCharacter.Level && Learned == 0;
    public bool CanBePracticed(IPlayableCharacter playableCharacter) => Level <= playableCharacter.Level && Learned > 0;
}
