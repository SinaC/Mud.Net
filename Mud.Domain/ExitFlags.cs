namespace Mud.Domain;

[Flags]
public enum ExitFlags
{
    None        = 0x00000000,
    Door        = 0x00000001,
    Closed      = 0x00000002,
    Locked      = 0x00000004,
    Easy        = 0x00000008,
    Hard        = 0x00000010,
    Hidden      = 0x00000020,
    PickProof   = 0x00000040,
    NoPass      = 0x00000080,
}
