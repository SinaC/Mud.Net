namespace Mud.Domain;

public enum CharacterAttributeAffectLocations
{
    None            = 0,
    Strength        = 1,
    Intelligence    = 2,
    Wisdom          = 3,
    Dexterity       = 4,
    Constitution    = 5,
    Characteristics = 6, // Strength + Intelligence + Wisdom + Dexterity + Constitution
    MaxHitPoints    = 7,
    SavingThrow     = 8,
    HitRoll         = 9,
    DamRoll         = 10,
    MaxMovePoints   = 11,
    ArmorBash       = 12,
    ArmorPierce     = 13,
    ArmorSlash      = 14,
    ArmorMagic      = 15,
    AllArmor        = 16,
}
