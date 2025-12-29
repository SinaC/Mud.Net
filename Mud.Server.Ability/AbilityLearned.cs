using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability;

public class AbilityLearned : IAbilityLearned
{
    public string Name { get; }

    public int Level { get; protected set; }

    public int Rating { get; protected set; }

    public IAbilityUsage AbilityUsage { get; }

    public int Learned { get; protected set; }

    public AbilityLearned(IAbilityUsage abilityUsage)
    {
        Name = abilityUsage.Name;
        Level = abilityUsage.Level;
        Rating = abilityUsage.Rating;
        AbilityUsage = abilityUsage;
        Learned = 0; // can be gained but not yet learned
    }

    public AbilityLearned(LearnedAbilityData learnedAbilityData, IAbilityUsage abilityUsage)
    {
        Name = learnedAbilityData.Name;
        Level = learnedAbilityData.Level;
        Rating = learnedAbilityData.Rating;
        AbilityUsage = abilityUsage;
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
            Level = Level,
            Learned = Learned,
            Rating = Rating,
        };
    }

    public void Update(int level, int rating, int learned)
    {
        Level = level;
        Rating = rating;
        Learned = learned;
    }

    public bool CanBePracticed(IPlayableCharacter playableCharacter)
        => Level <= playableCharacter.Level && Learned > 0;

    public bool HasCost => AbilityUsage.HasCost;
}
