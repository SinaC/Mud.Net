using System;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2
{
    public interface IAbility
    {
        int Id { get; }
        string Name { get; }
        int PulseWaitTime { get; }
        int Cooldown { get; }
        int LearnDifficultyMultiplier { get; }
        AbilityFlags Flags { get; }
        AbilityEffects Effects { get; }
    }

    [Flags]
    public enum AbilityEffects
    {
        Damage = 0x00000001,
        DamageArea = 0x00000002,
        Healing = 0x00000004,
        HealingArea = 0x00000008,
        Buff = 0x00000010,
        Debuff = 0x00000020,
        Cure = 0x00000040,
        Dispel = 0x00000080,
        Transportation = 0x00000100,
        Animation = 0x000000200,
        Creation = 0x00000400,
    }

    public enum AbilityTargetResults
    {
        MissingParameter,
        InvalidTarget,
        TargetNotFound,
        Ok,
        Error
    }
}
