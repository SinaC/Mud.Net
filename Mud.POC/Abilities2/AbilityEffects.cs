using System;

namespace Mud.POC.Abilities2
{
    [Flags]
    public enum AbilityEffects
    {
        None = 0x00000000,
        Damage = 0x00000001,
        DamageArea = 0x00000002,
        Healing = 0x00000004,
        HealingArea = 0x00000008,
        Buff = 0x00000010,
        Debuff = 0x00000020,
        Cure = 0x00000040,
        Dispel = 0x00000080,
        Transportation = 0x00000100,
        Animation = 0x00000200,
        Creation = 0x00000400,
        Detection = 0x00000800
    }
}
