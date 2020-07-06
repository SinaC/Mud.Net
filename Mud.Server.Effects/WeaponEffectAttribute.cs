using System;

namespace Mud.Server.Effects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WeaponEffectAttribute : Attribute
    {
        public string WeaponFlagName { get; }

        public WeaponEffectAttribute(string weaponFlagName)
        {
            WeaponFlagName = weaponFlagName;
        }
    }
}
