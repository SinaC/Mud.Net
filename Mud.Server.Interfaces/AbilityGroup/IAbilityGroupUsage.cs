namespace Mud.Server.Interfaces.AbilityGroup
{
    public interface IAbilityGroupUsage
    {
        string Name { get; }
        int Cost { get; }
        IAbilityGroupInfo AbilityGroupInfo { get; }
    }
}
