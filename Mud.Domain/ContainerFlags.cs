namespace Mud.Domain;

[Flags]
public enum ContainerFlags
{
    None        = 0x00000000,
    Closed      = 0x00000001,
    Locked      = 0x00000002,
    PickProof   = 0x00000004,
    NoClose     = 0x00000008,
    NoLock      = 0x00000010,
    Easy        = 0x00000020,
    Hard        = 0x00000040,
}
