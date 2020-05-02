using System;

namespace Mud.POC.Abilities
{
    [Flags]
    public enum AbilityFlags
    {
        None                = 0x00000000,
        Passive             = 0x00000001,
        AuraIsHidden        = 0x00000002,
        CannotMiss          = 0x00000004,
        CannotBeReflected   = 0x00000008,
    }
}
