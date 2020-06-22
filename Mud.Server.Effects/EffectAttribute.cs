using System;

namespace Mud.Server.Effects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EffectAttribute : Attribute
    {
        public string Name { get; }

        public EffectAttribute(string name)
        {
            Name = name;
        }
    }
}
