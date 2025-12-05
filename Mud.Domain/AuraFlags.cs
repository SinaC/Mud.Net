namespace Mud.Domain;

[Flags]
public enum AuraFlags
{
    None        = 0x0,
    StayDeath   = 0x1, // Remains even if affected dies
    NoDispel    = 0x2, // Can't be dispelled
    Permanent   = 0x4, // No duration
    Hidden      = 0x8, // Not displayed
}
