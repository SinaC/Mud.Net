using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability;

public interface IAbilityLearned
{
    string Name { get; }

    int Level { get; }

    ResourceKinds? ResourceKind { get; }

    int CostAmount { get; }

    CostAmountOperators CostAmountOperator { get; }

    int Rating { get; }

    IAbilityInfo AbilityInfo { get; }

    int Learned { get; }

    void IncrementLearned(int amount);
    void SetLearned(int amount);

    void Update(int level, int rating, int costAmount, int learned);
    void Update(int level, int rating, int learned);

    bool CanBeGained(IPlayableCharacter playableCharacter);
    bool CanBePracticed(IPlayableCharacter playableCharacter);

    LearnedAbilityData MapLearnedAbilityData();
}
