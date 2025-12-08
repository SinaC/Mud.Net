namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupManager
    {
        IAbilityGroupInfo? this[string abilityGroupName] { get; }
    }
}
