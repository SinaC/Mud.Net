namespace Mud.Server.Domain;

public enum CharacterAttributeAffectLocations
{
    None            = 0,
    Strength        = 1,
    Intelligence    = 2,
    Wisdom          = 3,
    Dexterity       = 4,
    Constitution    = 5,
    Characteristics = 6, // Strength + Intelligence + Wisdom + Dexterity + Constitution
    SavingThrow     = 7,
    HitRoll         = 8,
    DamRoll         = 9,
    ArmorBash       = 10,
    ArmorPierce     = 11,
    ArmorSlash      = 12,
    ArmorMagic      = 13,
    AllArmor        = 14,
}
