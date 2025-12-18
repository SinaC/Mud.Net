using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Ability;

public interface IAbilityDefinition
{
    AbilityTypes Type { get; }
    string Name { get; }
    AbilityEffects Effects { get; }
    int? PulseWaitTime { get; }
    int? CooldownInSeconds { get; }
    int LearnDifficultyMultiplier { get; }

    Type AbilityExecutionType { get; }

    string? Help { get; }
    string? OneLineHelp { get; }
    string[]? Syntax { get; }

    bool HasCharacterWearOffMessage { get; }
    string? CharacterWearOffMessage { get; }

    bool HasItemWearOffMessage { get; }
    string? ItemWearOffMessage { get; }

    bool IsDispellable { get; }
    string? DispelRoomMessage { get; }

    Shapes[]? AllowedShapes { get; }
}

public enum AbilityTypes
{
    Skill,
    Spell,
    Passive,
    Weapon
}

[Flags]
public enum AbilityEffects
{
    None = 0x00000000,
    Damage = 0x00000001,
    DamageArea = 0x00000002,
    Healing = 0x00000004,
    HealingArea = 0x00000008,
    Buff = 0x00000010,
    Debuff = 0x00000020,
    Cure = 0x00000040,
    Dispel = 0x00000080,
    Transportation = 0x00000100,
    Animation = 0x00000200,
    Creation = 0x00000400,
    Detection = 0x00000800,
    Enchantment = 0x00001000
}
