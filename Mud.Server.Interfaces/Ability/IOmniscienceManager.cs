namespace Mud.Server.Interfaces.Ability;

public interface IOmniscienceManager
{
    IEnumerable<IAbilityLearned> LearnedAbilities { get; }
    IAbilityLearned? this[string abilityName] { get; }
}