﻿using System;

namespace Mud.POC.Abilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class AbilityAttribute : Attribute
    {
        public const int DefaultPulseWaitTime = 12;

        public int Id { get; set; }
        public string Name { get; set; }
        public AbilityTargets Target { get; set; }
        public int PulseWaitTime { get; set; }
        public AbilityFlags Flags { get; set; }

        public AbilityAttribute(int id, string name, AbilityTargets target)
        {
            Flags = AbilityFlags.None;
            PulseWaitTime = DefaultPulseWaitTime;
        }
    }

    public class SpellAttribute : AbilityAttribute
    {
        public SpellAttribute(int id, string name, AbilityTargets target)
            : base(id, name, target)
        {
        }
    }

    //public class SkillAttribute : AbilityAttribute
    //{
    //    public SkillAttribute(int id, string name, AbilityTargets target)
    //        : base(id, name, target)
    //    {
    //        GenerateCommand = true;
    //    }
    //}
}
