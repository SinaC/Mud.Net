using System;

namespace Mud.Domain
{
    public enum Sex
    {
        Neutral     = 0,
        Male        = 1,
        Female      = 2,
    }

    public enum Positions
    {
        Stunned     = 0,
        Sleeping    = 1,
        Resting     = 2,
        Sitting     = 3,
        Fighting    = 4,
        Standing    = 5,
    }

    [Flags]
    public enum FurnitureActions
    {
        None  = 0x0000,
        Stand = 0x0001,
        Sit   = 0x0002,
        Rest  = 0x0004,
        Sleep = 0x0008,
    }

    public enum FurniturePlacePrepositions
    {
        None    = 0,
        At      = 1,
        In      = 2,
        On      = 3,
    }

    public enum Forms
    {
        Normal      = 0,
        Bear        = 1, // druid
        Cat         = 2, // druid
        Travel      = 3, // druid
        Moonkin     = 4, // druid
        Tree        = 5, // druid
        Shadow      = 6, // priest
    }

    // TODO
    //public enum Parts
    //{
    //    Head,
    //    Arm,
    //    Leg,
    //    Body,
    //    Hand,
    //    Foot,
    //    Finger,
    //    Ear,
    //    Eye,
    //    Heart,
    //    Brains,
    //    Guts,
    //    LongTongue,
    //    EyeStalks,
    //    Tentacles,
    //    Fins,
    //    Wings,
    //    Tail,
    //    Claws,
    //    Fangs,
    //    Horns,
    //    Scales,
    //    Tusks
    //}

    [Flags]
    public enum ItemFlags
    {
        None            = 0x0000,
        NoTake          = 0x0001, // Cannot take item
        NoDrop          = 0x0002, // Cannot be dropped once in inventory (cannot be put in container) [can be uncursed]
        NoRemove        = 0x0004, // Cannot be removed once equiped [can be uncursed]
        RotDeath        = 0x0008, // Disappear when holder dies
        Indestructible  = 0x0010, // No condition
        Humming         = 0x0020, // Humming
        Glowing         = 0x0040, // Glowing
        Invisible       = 0x0080, // Invisible
        MeltOnDrop      = 0x0100, // Melt when dropped
        NoPurge         = 0x0200, // Not deleted when purge command is used
    }

    public enum WearLocations
    {
        None        = 0,
        Light       = 1,
        Head        = 2,
        Amulet      = 3,
        Shoulders   = 4,
        Chest       = 5,
        Cloak       = 6,
        Waist       = 7,
        Wrists      = 8,
        Arms        = 9,
        Hands       = 10,
        Ring        = 11,
        Legs        = 12,
        Feet        = 13,
        Trinket     = 14,
        Wield       = 15,
        Hold        = 16,
        Shield      = 17,
        Wield2H     = 18,
    }

    public enum EquipmentSlots
    {
        None        = 0,
        Light       = 1,
        Head        = 2,
        Amulet      = 3,
        Shoulders   = 4,
        Chest       = 5,
        Cloak       = 6,
        Waist       = 7,
        Wrists      = 8,
        Arms        = 9,
        Hands       = 10,
        Ring        = 11,
        Legs        = 12,
        Feet        = 13,
        Trinket     = 14,
        // MainHand + OffHand are needed to equip Wield2H unless big enough
        MainHand    = 15, // can equip Wield
        OffHand     = 16, // can equip Wield/Hold/Shield
    }

    public enum ArmorKinds
    {
        Cloth       = 0,
        Leather     = 1,
        Mail        = 2,
        Plate       = 3,
    }

    public enum WeaponTypes
    {
        // one-handed
        Dagger      = 0,
        Fist        = 1,
        Axe1H       = 2,
        Mace1H      = 3,
        Sword1H     = 4,
        // two-handed
        Polearm     = 5,
        Stave       = 6,
        Axe2H       = 7,
        Mace2H      = 8,
        Sword2H     = 9,
    }

    public enum SchoolTypes
    {
        None        = 0,
        Physical    = 1,
        Arcane      = 2,
        Fire        = 3,
        Frost       = 4,
        Nature      = 5,
        Shadow      = 6,
        Holy        = 7,
    }

    public enum DispelTypes
    {
        None        = 0,
        Magic       = 1,
        Poison      = 2,
        Disease     = 3,
        Curse       = 4,
    }

    public enum PrimaryAttributeTypes
    {
        Strength    = 0,
        Agility     = 1,
        Stamina     = 2,
        Intellect   = 3,
        Spirit      = 4,
    }

    public enum SecondaryAttributeTypes // dependent on primary attribute, aura and items (computed in Recompute)
    {
        MaxHitPoints    = 0,
        AttackSpeed     = 1,
        AttackPower     = 2,
        SpellPower      = 3,
        Armor           = 4,
        Critical        = 5,
        Dodge           = 6,
        Parry           = 7,
        Block           = 8,
    }

    public enum ResourceKinds
    {
        None        = 0,
        Mana        = 1,
        Energy      = 2,
        Rage        = 3,
        Runic       = 4,
        // TODO: runes
        //BloodRune,
        //FrostRune,
        //UnholyRune,
        //DeathRune
    }

    public enum AmountOperators
    {
        None        = 0,
        Fixed       = 1,
        Percentage  = 2,
    }

    public enum AuraModifiers
    {
        None            = 0,
        Strength        = 1,
        Agility         = 2,
        Stamina         = 3,
        Intellect       = 4,
        Spirit          = 5,
        Characteristics = 6,
        AttackSpeed     = 7,
        AttackPower     = 8,
        SpellPower      = 9,
        MaxHitPoints    = 10,
        DamageAbsorb    = 11,
        HealAbsorb      = 12,
        Armor           = 13,
        Critical        = 14,
        Dodge           = 15,
        Parry           = 16,
        Block           = 17,
    }

    public enum AdminLevels
    {
        Angel       = 0,
        DemiGod     = 1,
        Immortal    = 2,
        God         = 3,
        Deity       = 4,
        Supremacy   = 5,
        Creator     = 6,
        Implementor = 7,
    }

    [Flags]
    public enum WiznetFlags
    {
        None      = 0x0000,
        Incarnate = 0x0001,
        Punish    = 0x0002,
        Logins    = 0x0004,
        Deaths    = 0x0008,
        MobDeaths = 0x0010,
        Levels    = 0x0020,
        Snoops    = 0x0040,
        Bugs      = 0x0080,
        Typos     = 0x0100,
        Help      = 0x0200,
        Load      = 0x0400,
        Promote   = 0x0800
    }
}
