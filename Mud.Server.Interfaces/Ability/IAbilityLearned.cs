using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability;

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

    bool CanBePracticed(IPlayableCharacter playableCharacter);

    bool HasCost { get; }

    LearnedAbilityData MapLearnedAbilityData();
}
