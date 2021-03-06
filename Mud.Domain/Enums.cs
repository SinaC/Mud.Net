﻿using System;

namespace Mud.Domain
{
    public enum Sex
    {
        Neutral     = 0,
        Male        = 1,
        Female      = 2,
    }

    //public enum Positions // Order is important
    //{
    //    Dead        = 0,
    //    Mortal      = 1,
    //    Incap       = 2,
    //    Stunned     = 3,
    //    Sleeping    = 4,
    //    Resting     = 5,
    //    Sitting     = 6,
    //    Fighting    = 7,
    //    Standing    = 8,
    //}
    public enum Positions
    {
        Sleeping = 0,
        Resting = 1,
        Sitting = 2,
        Standing = 3
    }

    public enum Conditions // Must starts at 0 and consecutive
    {
        Drunk   = 0,
        Full    = 1,
        Thirst  = 2,
        Hunger  = 3
    }

    [Flags]
    public enum FurnitureActions
    {
        None  = 0x00000000,
        Stand = 0x00000001,
        Sit   = 0x00000002,
        Rest  = 0x00000004,
        Sleep = 0x00000008,
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

    public enum WearLocations
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
        //Trinket     = 13,
        Wield       = 14,
        Hold        = 15,
        Shield      = 16,
        Wield2H     = 17,
        Float       = 18,
    }

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

    public enum WeaponTypes
    {
        Exotic      = 0,
        Sword       = 1,
        Dagger      = 2,
        Spear       = 3,
        Mace        = 4,
        Axe         = 5,
        Flail       = 6,
        Whip        = 7,
        Polearm     = 8,
        Staff       = 9,
    }

    public enum SchoolTypes
    {
        None            = 0,
        // Physical
        Bash            = 1,
        Pierce          = 2,
        Slash           = 3,
        // Magic
        Fire            = 4,
        Cold            = 5,
        Lightning       = 6,
        Acid            = 7,
        Poison          = 8,
        Negative        = 9,
        Holy            = 10,
        Energy          = 11,
        Mental          = 12,
        Disease         = 13,
        Drowning        = 14,
        Light           = 15,
        Other           = 16,
        Harm            = 17,
        Charm           = 18,
        Sound           = 19,
    }

    public enum CharacterAttributes // Must starts at 0 and consecutive
    {
        Strength        = 0,
        Intelligence    = 1,
        Wisdom          = 2,
        Dexterity       = 3,
        Constitution    = 4,
        MaxHitPoints    = 5,
        SavingThrow     = 6,
        HitRoll         = 7,
        DamRoll         = 8,
        MaxMovePoints   = 9,
        ArmorBash       = 10,
        ArmorPierce     = 11,
        ArmorSlash      = 12,
        ArmorExotic     = 13
    }

    public enum BasicAttributes // must have the same values as CharacterAttributes
    { 
        Strength        = CharacterAttributes.Strength,
        Intelligence    = CharacterAttributes.Intelligence,
        Wisdom          = CharacterAttributes.Wisdom,
        Dexterity       = CharacterAttributes.Dexterity,
        Constitution    = CharacterAttributes.Constitution,
    }

    public enum Armors // must have the same values as CharacterAttributes
    {
        Bash        = CharacterAttributes.ArmorBash,
        Pierce      = CharacterAttributes.ArmorPierce,
        Slash       = CharacterAttributes.ArmorSlash,
        Exotic      = CharacterAttributes.ArmorExotic,
    }

    public enum ResourceKinds // must starts at 0 and no hole (is used as index in array)
    {
        Mana        = 0,
        Psy         = 1,
    }

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

    public enum IRVAffectLocations
    {
        None                = 0,
        Immunities          = 1,
        Resistances         = 2,
        Vulnerabilities     = 3,
    }

    public enum ResistanceLevels 
    {
        None        = 0,
        Normal      = 1,
        Immune      = 2,
        Resistant   = 3,
        Vulnerable  = 4
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

    public enum CostAmountOperators
    {
        None        = 0,
        Fixed       = 1,
        Percentage  = 2
    }

    [Flags]
    public enum AuraFlags
    {
        None        = 0x0,
        StayDeath   = 0x1, // Remains even if affected dies
        NoDispel    = 0x2, // Can't be dispelled
        Permanent   = 0x4, // No duration
        Hidden      = 0x8, // Not displayed
    }

    public enum AffectOperators
    {
        Add     = 0,
        Or      = 1,
        Assign  = 2,
        Nor     = 3
    }

    public enum Sizes
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Giant
    }

    [Flags]
    public enum WiznetFlags
    {
        None      = 0x00000000,
        Incarnate = 0x00000001,
        Punish    = 0x00000002,
        Logins    = 0x00000004,
        Deaths    = 0x00000008,
        MobDeaths = 0x00000010,
        Levels    = 0x00000020,
        Snoops    = 0x00000040,
        Bugs      = 0x00000080,
        Typos     = 0x00000100,
        Help      = 0x00000200,
        Load      = 0x00000400,
        Promote   = 0x00000800,
        Resets    = 0x00001000,
        Restore   = 0x00002000,
        Immortal  = 0x00004000,
        Saccing   = 0x00008000,
    }

    [Flags]
    public enum ExitFlags
    {
        None        = 0x00000000,
        Door        = 0x00000001,
        Closed      = 0x00000002,
        Locked      = 0x00000004,
        Easy        = 0x00000008,
        Hard        = 0x00000010,
        Hidden      = 0x00000020,
        PickProof   = 0x00000040,
        NoPass      = 0x00000080,
    }

    [Flags]
    public enum PortalFlags
    {
        None        = 0x00000000,
        Closed      = 0x00000001,
        Locked      = 0x00000002,
        PickProof   = 0x00000004,
        NoClose     = 0x00000008,
        NoLock      = 0x00000010,
        NoCurse     = 0x00000020,
        GoWith      = 0x00000040,
        Buggy       = 0x00000080,
        Random      = 0x00000100,
        Easy        = 0x00000200,
        Hard        = 0x00000400,
    }

    [Flags]
    public enum ContainerFlags
    {
        None        = 0x00000000,
        Closed      = 0x00000001,
        Locked      = 0x00000002,
        PickProof   = 0x00000004,
        NoClose     = 0x00000008,
        NoLock      = 0x00000010,
        Easy        = 0x00000020,
        Hard        = 0x00000040,
    }

    public enum SunPhases
    {
        Dark,
        Rise,
        Light,
        Set
    }

    public enum SkyStates // Order is important
    {
        Cloudless,
        Cloudy,
        Raining,
        Lightning
    }

    public enum SectorTypes
    {
        Inside = 0,
        City = 1,
        Field = 2,
        Forest = 3,
        Hills = 4,
        Mountain = 5,
        WaterSwim = 6,
        WaterNoSwim = 7,
        Burning = 8,
        Air = 9,
        Desert = 10,
        Underwater = 11
    }

    [Flags]
    public enum AreaFlags
    {
        None = 0x0000,
        Changed = 0x0001,
        Added = 0x0002,
        Loading = 0x0004
    }

    [Flags]
    public enum AutoFlags
    {
        None      = 0x0000,
        Assist    = 0x0001,
        Exit      = 0x0002,
        Sacrifice = 0x0004,
        Gold      = 0x0008,
        Split     = 0x0010,
        Loot      = 0x0020,
    }
}
