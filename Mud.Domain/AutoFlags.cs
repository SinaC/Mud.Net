namespace Mud.Domain;

[Flags]
public enum AutoFlags
{
    None      = 0x0000,
    Assist    = 0x0001,
    Exit      = 0x0002,
    Sacrifice = 0x0004,
    Gold      = 0x0008,
    Split     = 0x0010,
    Loot      = 0x0020,
    Affect    = 0x0040,
}
