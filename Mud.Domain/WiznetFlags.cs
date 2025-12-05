namespace Mud.Domain;

[Flags]
public enum WiznetFlags
{
    None      = 0x00000000,
    Incarnate = 0x00000001,
    Punish    = 0x00000002,
    Logins    = 0x00000004,
    Deaths    = 0x00000008,
    MobDeaths = 0x00000010,
    Levels    = 0x00000020,
    Snoops    = 0x00000040,
    Bugs      = 0x00000080,
    Typos     = 0x00000100,
    Help      = 0x00000200,
    Load      = 0x00000400,
    Promote   = 0x00000800,
    Resets    = 0x00001000,
    Restore   = 0x00002000,
    Immortal  = 0x00004000,
    Saccing   = 0x00008000,
}
