using Mud.Common;
using Mud.Logger;
using Mud.Server.Interfaces.Ability;
using System.Reflection;

namespace Mud.Server.Ability;

public class AbilityInfo : IAbilityInfo
{
    #region IAbilityInfo

    public AbilityTypes Type { get; }
    public string Name { get; }
    public AbilityEffects Effects { get; }
    public int? PulseWaitTime { get; }
    public int? CooldownInSeconds { get; }
    public int LearnDifficultyMultiplier { get; }
    public Type AbilityExecutionType { get; }

    public bool HasCharacterWearOffMessage { get; }
    public string? CharacterWearOffMessage { get; }

    public bool HasItemWearOffMessage { get; }
    public string? ItemWearOffMessage { get; }

    public bool IsDispellable { get; }
    public string? DispelRoomMessage { get; }

    #endregion

    public AbilityInfo(Type abilityExecutionType)
    {
        AbilityExecutionType = abilityExecutionType;

        var abilityBaseAttribute = abilityExecutionType.GetCustomAttribute<AbilityBaseAttribute>();

        Type = abilityBaseAttribute!.Type;
        string pascalCaseName = abilityBaseAttribute.Name.ToPascalCase();
        if (pascalCaseName != abilityBaseAttribute.Name)
            Log.Default.WriteLine(LogLevels.Warning, "Ability: {0} is not a valid name, its has been modified to {1}.", abilityBaseAttribute.Name, pascalCaseName);
        Name = pascalCaseName;
        Effects = abilityBaseAttribute.Effects;
        PulseWaitTime = (abilityBaseAttribute as ActiveAbilityBaseAttribute)?.PulseWaitTime;
        CooldownInSeconds = abilityBaseAttribute.CooldownInSeconds <= 0
            ? (int?)null
            : abilityBaseAttribute.CooldownInSeconds;
        LearnDifficultyMultiplier = abilityBaseAttribute.LearnDifficultyMultiplier;

        var additionalInfoAttributes = abilityExecutionType.GetCustomAttributes<AbilityAdditionalInfoAttribute>().ToArray();
        HasCharacterWearOffMessage = additionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().Any();
        CharacterWearOffMessage = additionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().SingleOrDefault()?.Message;
        HasItemWearOffMessage = additionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().Any();
        ItemWearOffMessage = additionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().SingleOrDefault()?.HolderMessage;
        IsDispellable = additionalInfoAttributes.OfType<AbilityDispellableAttribute>().Any();
        DispelRoomMessage = additionalInfoAttributes.OfType<AbilityDispellableAttribute>().SingleOrDefault()?.RoomMessage;
    }
}
