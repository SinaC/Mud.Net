using System;

namespace Mud.POC.Abilities
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
        public string CharacterDispelMessage { get; set; }
        public string ItemDispelMessage { get; set; }

        protected AbilityAttribute(int id, string name, AbilityTargets target)
        {
            Id = id;
            Name = name;
            Target = target;
            PulseWaitTime = DefaultPulseWaitTime;
            Flags = AbilityFlags.None;
            CharacterDispelMessage = null;
            ItemDispelMessage = null;
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
}
