using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability;

public class AbilityLearned : AbilityUsage, IAbilityLearned
{
    private ILogger Logger { get; }
    public int Learned { get; protected set; }

    public AbilityLearned(ILogger logger, IAbilityUsage abilityUsage)
        : base(abilityUsage.Name, abilityUsage.Level, abilityUsage.ResourceKind, abilityUsage.CostAmount, abilityUsage.CostAmountOperator, abilityUsage.Rating, abilityUsage.AbilityInfo)
    {
        Logger = logger;

        Learned = 0; // can be gained but not yet learned
    }

    public AbilityLearned(ILogger logger, LearnedAbilityData learnedAbilityData, IAbilityInfo abilityInfo)
        : base(learnedAbilityData.Name, learnedAbilityData.Level, learnedAbilityData.ResourceKind, learnedAbilityData.CostAmount, learnedAbilityData.CostAmountOperator, learnedAbilityData.Rating, abilityInfo)
    {
        Logger = logger;

        Learned = learnedAbilityData.Learned;
    }

    public void IncrementLearned(int amount)
    {
        if (amount <= 0)
        {
            Logger.LogError("Trying to decrement learned of ability {Name} by {amount}.", Name, amount);
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
