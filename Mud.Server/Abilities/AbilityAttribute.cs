using Mud.Domain;
using System;

namespace Mud.Server.Abilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
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
        public int LearnDifficultyMultiplier { get; set; } //

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
            LearnDifficultyMultiplier = 1;
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
