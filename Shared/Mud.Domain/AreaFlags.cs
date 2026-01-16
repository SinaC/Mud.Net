namespace Mud.Domain;

[Flags]
public enum AreaFlags
{
    None    = 0x00000000,
    Changed = 0x00000001,
    Added   = 0x00000002,
    Loading = 0x00000004
}
