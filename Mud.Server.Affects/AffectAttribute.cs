using System;

namespace Mud.Server.Affects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AffectAttribute : Attribute
    {
        public string Name { get; }

        public AffectAttribute(string name)
        {
            Name = name;
        }
    }
}
