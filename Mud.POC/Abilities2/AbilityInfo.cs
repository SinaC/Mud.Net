﻿using System;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Abilities2
{
    public class AbilityInfo
    {
        public string Name { get; }
        public AbilityEffects Effects { get; }
        public int PulseWaitTime { get; }
        public int? Cooldown { get; }
        public int LearnDifficultyMultiplier { get; }

        public AbilityAdditionalInfoAttribute[] AdditionalInfoAttributes { get; }

        public Type AbilityExecutionType { get; }

        public AbilityInfo(Type abilityExecutionType)
        {
            AbilityExecutionType = abilityExecutionType;

            AbilityBaseAttribute abilityBaseAttribute = abilityExecutionType.GetCustomAttribute<AbilityBaseAttribute>();
            Name = abilityBaseAttribute.Name;
            Effects = abilityBaseAttribute.Effects;
            PulseWaitTime = abilityBaseAttribute.PulseWaitTime;
            Cooldown = abilityBaseAttribute.Cooldown;
            LearnDifficultyMultiplier = abilityBaseAttribute.LearnDifficultyMultiplier;

            AdditionalInfoAttributes = abilityExecutionType.GetCustomAttributes<AbilityAdditionalInfoAttribute>().ToArray();
        }

        public bool HasCharacterWearOffMessage => AdditionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().Any();
        public string CharacterWearOffMessage => AdditionalInfoAttributes.OfType<AbilityCharacterWearOffMessageAttribute>().SingleOrDefault()?.Message;

        public bool HasItemWearOffMessage => AdditionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().Any();
        public string ItemWearOffMessage => AdditionalInfoAttributes.OfType<AbilityItemWearOffMessageAttribute>().SingleOrDefault()?.HolderMessage;

        public bool IsDispellable => AdditionalInfoAttributes.OfType<AbilityDispellableAttribute>().Any();
        public string DispelRoomMessage => AdditionalInfoAttributes.OfType<AbilityDispellableAttribute>().SingleOrDefault()?.RoomMessage;
    }
}
