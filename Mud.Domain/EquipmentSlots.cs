namespace Mud.Domain;

public enum EquipmentSlots
{
    None        = 0,
    Light       = 1,
    Head        = 2,
    Amulet      = 3,
    Chest       = 4,
    Cloak       = 5,
    Waist       = 6,
    Wrists      = 7,
    Arms        = 8,
    Hands       = 9,
    Ring        = 10,
    Legs        = 11,
    Feet        = 12,
    // MainHand + OffHand are needed to equip Wield2H unless big enough
    MainHand    = 13, // can equip Wield
    OffHand     = 14, // can equip Wield/Hold/Shield
    Float       = 15,
}
