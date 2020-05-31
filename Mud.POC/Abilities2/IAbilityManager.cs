namespace Mud.POC.Abilities2
{
    public interface IAbilityManager
    {
        IAbility this[string abilityName] { get; }
    }
}
