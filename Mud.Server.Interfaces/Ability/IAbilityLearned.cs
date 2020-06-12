namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityLearned : IAbilityUsage
    {
        int Learned { get; }
    }
}
