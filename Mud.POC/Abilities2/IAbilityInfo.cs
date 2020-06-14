using System;

namespace Mud.POC.Abilities2
{
    public interface IAbilityInfo
    {
        AbilityTypes Type { get; }
        string Name { get; }
        AbilityEffects Effects { get; }
        int? PulseWaitTime { get; }
        int? Cooldown { get; }
        int LearnDifficultyMultiplier { get; }

        Type AbilityExecutionType { get; }

        bool HasCharacterWearOffMessage { get; }
        string CharacterWearOffMessage { get; }

        bool HasItemWearOffMessage { get; }
        string ItemWearOffMessage { get; }

        bool IsDispellable { get; }
        string DispelRoomMessage { get; }
    }

    public enum AbilityTypes
    {
        Skill,
        Spell,
        Passive
    }
}
