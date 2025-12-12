namespace Mud.Domain;

[Flags]
public enum AuraFlags
{
    None        = 0x00,
    StayDeath   = 0x01, // Remains even if affected dies
    NoDispel    = 0x02, // Can't be dispelled
    Permanent   = 0x04, // No duration
    Hidden      = 0x08, // Not displayed
    Shapeshift  = 0x10, // must be unique
}
