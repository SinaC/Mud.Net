using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using System.Reflection;

namespace Mud.Server.Ability;

public class AbilityDefinition : IAbilityDefinition
{
    #region IAbilityDefinition

    public AbilityTypes Type { get; }
    public string Name { get; }
    public AbilityEffects Effects { get; }
    public int? PulseWaitTime { get; }
    public int? CooldownInSeconds { get; }
    public int LearnDifficultyMultiplier { get; }
    public Positions? MinPosition { get; }
    public bool? NotInCombat { get; }

    public Type AbilityExecutionType { get; }

    public string? Help { get; }
    public string? OneLineHelp { get; }
    public string[]? Syntax { get; }

    public bool HasCharacterWearOffMessage { get; }
    public string? CharacterWearOffMessage { get; }

    public bool HasItemWearOffMessage { get; }
    public string? ItemWearOffMessage { get; }

    public bool IsDispellable { get; }
    public string? DispelRoomMessage { get; }

    #endregion

    public AbilityDefinition(Type abilityExecutionType)
    {
        AbilityExecutionType = abilityExecutionType;

        var abilityBaseAttribute = abilityExecutionType.GetCustomAttribute<AbilityBaseAttribute>();
        Type = abilityBaseAttribute!.Type;
        Name = abilityBaseAttribute.Name.ToLowerInvariant();
        Effects = abilityBaseAttribute.Effects;
        PulseWaitTime = (abilityBaseAttribute as ActiveAbilityBaseAttribute)?.PulseWaitTime;
        CooldownInSeconds = abilityBaseAttribute.CooldownInSeconds <= 0
            ? (int?)null
            : abilityBaseAttribute.CooldownInSeconds;
        LearnDifficultyMultiplier = abilityBaseAttribute.LearnDifficultyMultiplier;

        Help = abilityExecutionType.GetCustomAttribute<HelpAttribute>()?.Help;
        OneLineHelp = abilityExecutionType.GetCustomAttribute<OneLineHelpAttribute>()?.OneLineHelp;
        Syntax = abilityExecutionType.GetCustomAttribute<SyntaxAttribute>()?.Syntax;

        var additionalInfoAttributes = abilityExecutionType.GetCustomAttributes<AbilityAdditionalInfoAttribute>().ToArray();
        HasCharacterWearOffMessage = additionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().Any();
        CharacterWearOffMessage = additionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().SingleOrDefault()?.Message;
        HasItemWearOffMessage = additionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().Any();
        ItemWearOffMessage = additionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().SingleOrDefault()?.HolderMessage;
        IsDispellable = additionalInfoAttributes.OfType<AbilityDispellableAttribute>().Any();
        DispelRoomMessage = additionalInfoAttributes.OfType<AbilityDispellableAttribute>().SingleOrDefault()?.RoomMessage;
    }
}
