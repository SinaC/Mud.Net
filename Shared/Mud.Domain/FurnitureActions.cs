namespace Mud.Domain;

[Flags]
public enum FurnitureActions
{
    None  = 0x00000000,
    Stand = 0x00000001,
    Sit   = 0x00000002,
    Rest  = 0x00000004,
    Sleep = 0x00000008,
}
