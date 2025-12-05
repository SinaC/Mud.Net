namespace Mud.Domain;

[Flags]
public enum AreaFlags
{
    None = 0x0000,
    Changed = 0x0001,
    Added = 0x0002,
    Loading = 0x0004
}
