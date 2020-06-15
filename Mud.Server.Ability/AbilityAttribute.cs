using Mud.Server.Interfaces.Ability;
using System;

namespace Mud.Server.Ability
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AbilityBaseAttribute : Attribute
    {
        public abstract AbilityTypes Type { get; }
        public string Name { get; set; }
        public AbilityEffects Effects { get; set; }
        public int CooldownInSeconds { get; set; }
        public int LearnDifficultyMultiplier { get; set; }

        protected AbilityBaseAttribute(string name, AbilityEffects effects)
        {
            Name = name;
            Effects = effects;
            CooldownInSeconds = -1;
            LearnDifficultyMultiplier = 1;
        }
    }

    public abstract class ActiveAbilityBaseAttribute : AbilityBaseAttribute
    {
        public int PulseWaitTime { get; set; }

        protected ActiveAbilityBaseAttribute(string name, AbilityEffects effects)
            : base(name, effects)
        {
            PulseWaitTime = 12;
        }
    }

    public class SpellAttribute : ActiveAbilityBaseAttribute
    {
        public override AbilityTypes Type => AbilityTypes.Spell;

        public SpellAttribute(string name, AbilityEffects effects)
            : base(name, effects)
        {
        }
    }

    public class SkillAttribute : ActiveAbilityBaseAttribute
    {
        public override AbilityTypes Type => AbilityTypes.Skill;

        public SkillAttribute(string name, AbilityEffects effects)
            : base(name, effects)
        {
        }
    }

    public class PassiveAttribute : AbilityBaseAttribute
    {
        public override AbilityTypes Type => AbilityTypes.Passive;

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
