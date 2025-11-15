using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability;

public interface IAbilityLearned : IAbilityUsage
{
    int Learned { get; }

    void IncrementLearned(int amount);

    LearnedAbilityData MapLearnedAbilityData();

    bool CanBeGained(IPlayableCharacter playableCharacter);
    bool CanBePracticed(IPlayableCharacter playableCharacter);
}
