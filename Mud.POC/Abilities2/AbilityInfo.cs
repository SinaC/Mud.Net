using Mud.Common;
using Mud.Logger;
using System;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Abilities2
{
    public class AbilityInfo : IAbilityInfo
    {
        #region IAbilityInfo

        public AbilityTypes Type { get; }
        public string Name { get; }
        public AbilityEffects Effects { get; }
        public int? PulseWaitTime { get; }
        public int? Cooldown { get; }
        public int LearnDifficultyMultiplier { get; }
        public Type AbilityExecutionType { get; }

        public bool HasCharacterWearOffMessage => AdditionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().Any();
        public string CharacterWearOffMessage => AdditionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().SingleOrDefault()?.Message;

        public bool HasItemWearOffMessage => AdditionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().Any();
        public string ItemWearOffMessage => AdditionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().SingleOrDefault()?.HolderMessage;

        public bool IsDispellable => AdditionalInfoAttributes.OfType<AbilityDispellableAttribute>().Any();
        public string DispelRoomMessage => AdditionalInfoAttributes.OfType<AbilityDispellableAttribute>().SingleOrDefault()?.RoomMessage;

        #endregion

        public AbilityInfo(Type abilityExecutionType)
        {
            AbilityExecutionType = abilityExecutionType;

            AbilityBaseAttribute abilityBaseAttribute = abilityExecutionType.GetCustomAttribute<AbilityBaseAttribute>();

            Type = abilityBaseAttribute.Type;
            string pascalCaseName = abilityBaseAttribute.Name.ToPascalCase();
            if (pascalCaseName != abilityBaseAttribute.Name)
                Log.Default.WriteLine(LogLevels.Warning, "Ability: {0} is not a valid name, its has been modified to {1}.", abilityBaseAttribute.Name, pascalCaseName);
            Name = pascalCaseName;
            Effects = abilityBaseAttribute.Effects;
            PulseWaitTime = (abilityBaseAttribute as ActiveAbilityBaseAttribute)?.PulseWaitTime;
            Cooldown = abilityBaseAttribute.Cooldown <= 0
                ? (int?)null
                : abilityBaseAttribute.Cooldown;
            LearnDifficultyMultiplier = abilityBaseAttribute.LearnDifficultyMultiplier;

            AdditionalInfoAttributes = abilityExecutionType.GetCustomAttributes<AbilityAdditionalInfoAttribute>().ToArray();
        }

        protected AbilityAdditionalInfoAttribute[] AdditionalInfoAttributes { get; }
    }
}
