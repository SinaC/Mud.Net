using Mud.Domain;
using System;

namespace Mud.Server.Ability
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AbilityAttribute : Attribute
    {
        public const int DefaultPulseWaitTime = 12;

        public int Id { get; }
        public string Name { get; }
        public AbilityTargets Target { get; }
        public int PulseWaitTime { get; set; }
        public AbilityFlags Flags { get; set; }
        public string CharacterWearOffMessage { get; set; } // Inform character about aura worn off (+dispel)
        public string ItemWearOffMessage { get; set; } // Inform item hold about aura worn off (+dispel)
        public string DispelRoomMessage { get; set; } // Used to inform room about spell being dispelled
        public string DamageNoun { get; set; } // Used by AbilityDamage, to specify if damage shouldn't be labelled with same name as ability
        public int LearnDifficultyMultiplier { get; set; } // multiplier when calling CheckImprove
        public int Cooldown { get; set; }

        protected AbilityAttribute(int id, string name, AbilityTargets target)
        {
            Id = id;
            Name = name;
            Target = target;
            PulseWaitTime = DefaultPulseWaitTime;
            Flags = AbilityFlags.None;
            CharacterWearOffMessage = null;
            ItemWearOffMessage = null;
            DispelRoomMessage = null;
            DamageNoun = null;
            LearnDifficultyMultiplier = 1;
            Cooldown = 0;
        }
    }

    public class SpellAttribute : AbilityAttribute
    {
        public SpellAttribute(int id, string name, AbilityTargets target)
            : base(id, name, target)
        {
        }
    }

    public class SkillAttribute : AbilityAttribute
    {
        public SkillAttribute(int id, string name, AbilityTargets target)
            : base(id, name, target)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PassiveListAttribute : Attribute
    {
    }
}
