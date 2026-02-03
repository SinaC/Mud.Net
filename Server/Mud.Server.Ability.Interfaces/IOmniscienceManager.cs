namespace Mud.Server.Ability.Interfaces;

public interface IOmniscienceManager
{
    IEnumerable<IAbilityLearned> LearnedAbilities { get; }
    IAbilityLearned? this[string abilityName] { get; }
}