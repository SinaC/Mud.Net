using System;

namespace Mud.POC.Abilities2
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AbilityBaseAttribute : Attribute
    {
        public string Name { get; set; }
        public AbilityEffects Effects { get; set; }
        public int PulseWaitTime { get; set; }
        public int? Cooldown { get; set; }
        public int LearnDifficultyMultiplier { get; set; }

        protected AbilityBaseAttribute(string name, AbilityEffects effects)
        {
            Name = name;
            Effects = effects;
            PulseWaitTime = 12;
            Cooldown = null;
            LearnDifficultyMultiplier = 1;
        }
    }

    public class SpellAttribute : AbilityBaseAttribute
    {
        public SpellAttribute(string name, AbilityEffects effects)
            : base(name, effects)
        {
        }
    }

    public class SkillAttribute : AbilityBaseAttribute
    {
        public SkillAttribute(string name, AbilityEffects effects)
            : base(name, effects)
        {
        }
    }

    public class PassiveAttribute : AbilityBaseAttribute
    {
        public PassiveAttribute(string name)
            : base(name, AbilityEffects.None)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AbilityAdditionalInfoAttribute : Attribute
    {
    }

    public class AbilityCharacterWearOffMessageAttribute : AbilityAdditionalInfoAttribute
    {
        public string Message { get; set; } // displayed to character

        public AbilityCharacterWearOffMessageAttribute(string message)
        {
            Message = message;
        }
    }

    public class AbilityItemWearOffMessageAttribute : AbilityAdditionalInfoAttribute
    {
        public string HolderMessage { get; set; } // displayed to holder

        public AbilityItemWearOffMessageAttribute(string holderMessage)
        {
            HolderMessage = holderMessage;

        }
    }

    public class AbilityDispellableAttribute : AbilityAdditionalInfoAttribute
    {
        public string RoomMessage { get; set; } // displayed to room

        public AbilityDispellableAttribute()
        {
        }

        public AbilityDispellableAttribute(string roomMessage)
        {
            RoomMessage = roomMessage;
        }
    }
}
