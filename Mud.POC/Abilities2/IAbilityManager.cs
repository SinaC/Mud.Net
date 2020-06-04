namespace Mud.POC.Abilities2
{
    public interface IAbilityManager
    {
        AbilityInfo this[string abilityName] { get; }
    }
}
