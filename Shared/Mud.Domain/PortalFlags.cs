namespace Mud.Domain;

[Flags]
public enum PortalFlags
{
    None        = 0x00000000,
    Closed      = 0x00000001,
    Locked      = 0x00000002,
    PickProof   = 0x00000004,
    NoClose     = 0x00000008,
    NoLock      = 0x00000010,
    NoCurse     = 0x00000020,
    GoWith      = 0x00000040,
    Buggy       = 0x00000080,
    Random      = 0x00000100,
    Easy        = 0x00000200,
    Hard        = 0x00000400,
}
