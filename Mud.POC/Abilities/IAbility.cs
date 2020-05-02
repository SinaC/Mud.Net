namespace Mud.POC.Abilities
{
    public interface IAbility
    {
        int Id { get; }

        string Name { get; }

        AbilityTargets Target { get; }

        int PulseWaitTime { get; }

        AbilityFlags AbilityFlags { get; }

        AbilityMethodInfo AbilityMethodInfo { get; }
    }
}
