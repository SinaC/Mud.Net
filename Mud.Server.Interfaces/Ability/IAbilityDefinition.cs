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
