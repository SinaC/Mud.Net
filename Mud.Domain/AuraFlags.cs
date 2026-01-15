namespace Mud.Domain;

[Flags]
public enum AuraFlags
{
    None        = 0x00000000,
    StayDeath   = 0x00000001, // Remains even if affected dies
    NoDispel    = 0x00000002, // Can't be dispelled
    Permanent   = 0x00000004, // No duration
    Hidden      = 0x00000008, // Not displayed
    Shapeshift  = 0x00000010, // must be unique
    Inherent    = 0x00000020, // no related to an ability
}
