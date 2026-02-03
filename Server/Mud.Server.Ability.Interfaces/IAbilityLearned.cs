using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Ability.Interfaces;

public interface IAbilityLearned
{
    string Name { get; }

    int Level { get; }

    int Rating { get; }

    IAbilityUsage AbilityUsage { get; }

    int Learned { get; }

    void IncrementLearned(int amount);
    void SetLearned(int amount);

    void Update(int level, int rating, int learned);

    bool HasCost { get; }

    LearnedAbilityData MapLearnedAbilityData();
}
