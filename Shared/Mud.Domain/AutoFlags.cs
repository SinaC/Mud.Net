namespace Mud.Domain;

[Flags]
public enum AutoFlags
{
    None      = 0x00000000,
    Assist    = 0x00000001,
    Exit      = 0x00000002,
    Sacrifice = 0x00000004,
    Gold      = 0x00000008,
    Split     = 0x00000010,
    Loot      = 0x00000020,
    Affect    = 0x00000040,
}
