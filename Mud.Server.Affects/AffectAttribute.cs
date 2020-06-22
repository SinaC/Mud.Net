using System;

namespace Mud.Server.Affects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AffectAttribute : Attribute
    {
        public string Name { get; }

        public AffectAttribute(string name)
        {
            Name = name;
        }
    }
}
