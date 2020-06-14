using System;

namespace Mud.Domain
{
    public enum Sex
    {
        Neutral     = 0,
        Male        = 1,
        Female      = 2,
    }

    public enum Positions // Order is important
    {
        Dead        = 0,
        Mortal      = 1,
        Incap       = 2,
        Stunned     = 3,
        Sleeping    = 4,
        Resting     = 5,
        Sitting     = 6,
        Fighting    = 7,
        Standing    = 8,
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
        None            = 0x00000000,
        Glowing         = 0x00000001,
        Humming         = 0x00000002,
        Dark            = 0x00000004,
        Lock            = 0x00000008,
        Evil            = 0x00000010,
        Invis           = 0x00000020,
        Magic           = 0x00000040,
        NoDrop          = 0x00000080, // Cannot be dropped once in inventory (cannot be put in container) [can be uncursed]
        Bless           = 0x00000100,
        AntiGood        = 0x00000200,
        AntiEvil        = 0x00000400,
        AntiNeutral     = 0x00000800,
        NoRemove        = 0x00001000, // Cannot be removed once equipped [can be uncursed]
        Inventory       = 0x00002000,
        NoPurge         = 0x00004000,
        RotDeath        = 0x00008000, // Disappear when holder dies
        VisibleDeath    = 0x00010000, // Visible when holder dies
        // not used
        NonMetal        = 0x00040000,
        NoLocate        = 0x00080000,
        MeltOnDrop      = 0x00100000, // Melt when dropped
        HadTimer        = 0x00200000,
        SellExtract     = 0x00400000,
        // not used
        BurnProof       = 0x01000000,
        NoUncurse       = 0x02000000,
        NoSacrifice     = 0x04000000,
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

    [Flags]
    public enum WeaponFlags
    {
        None        = 0x00000000,
        Flaming     = 0x00000001,
        Frost       = 0x00000002,
        Vampiric    = 0x00000004,
        Sharp       = 0x00000008,
        Vorpal      = 0x00000010,
        TwoHands    = 0x00000020, // TODO: remove
        Shocking    = 0x00000040,
        Poison      = 0x00000080,
        Holy        = 0x00000100,
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

    [Flags]
    public enum IRVFlags
    {
        None        = 0x00000000,
        Summon      = 0x00000001,
        Charm       = 0x00000002,
        Magic       = 0x00000004,
        Weapon      = 0x00000008,
        Bash        = 0x00000010,
        Pierce      = 0x00000020,
        Slash       = 0x00000040,
        Fire        = 0x00000080,
        Cold        = 0x00000100,
        Lightning   = 0x00000200,
        Acid        = 0x00000400,
        Poison      = 0x00000800,
        Negative    = 0x00001000,
        Holy        = 0x00002000,
        Energy      = 0x00004000,
        Mental      = 0x00008000,
        Disease     = 0x00010000,
        Drowning    = 0x00020000,
        Light       = 0x00040000,
        Sound       = 0x00080000,
        Wood        = 0x00100000,
        Silver      = 0x00200000,
        Iron        = 0x00400000,
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

    [Flags]
    public enum CharacterFlags
    {
        None            = 0x00000000,
        Blind           = 0x00000001,
        Invisible       = 0x00000002,
        DetectEvil      = 0x00000004,
        DetectInvis     = 0x00000008,
        DetectMagic     = 0x00000010,
        DetectHidden    = 0x00000020,
        DetectGood      = 0x00000040,
        Sanctuary       = 0x00000080,
        FaerieFire      = 0x00000100,
        Infrared        = 0x00000200,
        Curse           = 0x00000400,
        // Unused
        Poison          = 0x00001000,
        ProtectEvil     = 0x00002000,
        ProtectGood     = 0x00004000,
        Sneak           = 0x00008000,
        Hide            = 0x00010000,
        Sleep           = 0x00020000,
        Charm           = 0x00040000,
        Flying          = 0x00080000,
        PassDoor        = 0x00100000,
        Haste           = 0x00200000,
        Calm            = 0x00400000,
        Plague          = 0x00800000,
        Weaken          = 0x01000000,
        DarkVision      = 0x02000000,
        Berserk         = 0x04000000,
        Swim            = 0x08000000,
        Regeneration    = 0x10000000,
        Slow            = 0x20000000,
    }

    [Flags]
    public enum RoomFlags
    {
        None        = 0x00000000,
        Dark        = 0x00000001,
        // not used
        NoMob       = 0x00000004,
        Indoors     = 0x00000008,
        NoScan      = 0x00000010,
        // not used
        // not used
        // not used
        // not used
        Private     = 0x00000200,
        Safe        = 0x00000400,
        Solitary    = 0x00000800,
        // PetShop
        NoRecall    = 0x00002000,
        ImpOnly     = 0x00004000,
        GodsOnly    = 0x00008000,
        // HeroesOnly
        NewbiesOnly = 0x00020000, // level <= 5 only
        Law         = 0x00040000,
        NoWhere     = 0x00080000,
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
    public enum ActFlags 
    {
        None            = 0x00000000,
        Sentinel        = 0x00000001,
        Scavenger       = 0x00000002,
        StayArea        = 0x00000004,
        Aggressive      = 0x00000008,
        Wimpy           = 0x00000010,
        Pet             = 0x00000020,
        Undead          = 0x00000040,
        NoAlign         = 0x00000080,
        NoPurge         = 0x00000100,
        Outdoors        = 0x00000200,
        Indoors         = 0x00000400,
        UpdateAlways    = 0x00000800,
        Train           = 0x00001000,
        IsHealer        = 0x00002000,
        Gain            = 0x00004000,
        Practice        = 0x00008000,
        Aware           = 0x00010000,
        Warrior         = 0x00020000,
        Thief           = 0x00040000,
        Cleric          = 0x00080000,
        Mage            = 0x00100000,
    }

    [Flags]
    public enum OffensiveFlags
    {
        None            = 0x00000000,
        AreaAttack      = 0x00000001,
        Backstab        = 0x00000002,
        Bash            = 0x00000003,
        Berserk         = 0x00000008,
        Disarm          = 0x00000010,
        Dodge           = 0x00000020,
        Fade            = 0x00000040,
        Fast            = 0x00000080,
        Kick            = 0x00000100,
        DirtKick        = 0x00000200,
        Parry           = 0x00000400,
        Rescue          = 0x00000800,
        Tail            = 0x00001000,
        Trip            = 0x00002000,
        Crush           = 0x00004000,
        Bite            = 0x00008000,
    }

    [Flags]
    public enum AssistFlags
    {
        None            = 0x00000000,
        All             = 0x00000001,
        Align           = 0x00000002,
        Race            = 0x00000004,
        Players         = 0x00000008,
        Guard           = 0x00000010,
        Vnum            = 0x00000020,
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

    [Flags]
    public enum BodyForms
    {
        Edible       = 0x00000001,
        Poison       = 0x00000002,
        Magical      = 0x00000004,
        InstantDecay = 0x00000008,
        Other        = 0x00000010, // defined by material
        // 0x00000020
        Animal       = 0x00000040,
        Sentient     = 0x00000080,
        Undead       = 0x00000100,
        Construct    = 0x00000200,
        Mist         = 0x00000400,
        Intangible   = 0x00000800,

        Biped        = 0x00001000,
        Centaur      = 0x00002000,
        Insect       = 0x00004000,
        Spider       = 0x00008000,
        Crustacean   = 0x00010000,
        Worm         = 0x00020000,
        Blob         = 0x00040000,
        //0x00080000
        //0x00100000
        Mammal       = 0x00080000,
        Bird         = 0x00400000,
        Reptile      = 0x00800000,
        Snake        = 0x01000000,
        Dragon       = 0x02000000,
        Amphibian    = 0x04000000,
        Fish         = 0x08000000,
        ColdBlood    = 0x10000000,
        Fur          = 0x20000000,
        FourArms     = 0x40000000,
    }

    [Flags]
    public enum BodyParts
    {
        Head            = 0x00000001,
        Arms            = 0x00000002,
        Legs            = 0x00000004,
        Heart           = 0x00000008,
        Brains          = 0x00000010,
        Guts            = 0x00000020,
        Hands           = 0x00000040,
        Feet            = 0x00000080,
        Fingers         = 0x00000100,
        Ear             = 0x00000200,
        Eye             = 0x00000400,
        LongTongue      = 0x00000800,
        Eyestalks       = 0x00001000,
        Tentacles       = 0x00002000,
        Fins            = 0x00004000,
        Wings           = 0x00008000,
        Tail            = 0x00010000,
        Body            = 0x00020000,
        // 0x00040000
        // 0x00080000
        Claws           = 0x00100000,
        Fangs           = 0x00200000,
        Horns           = 0x00400000,
        Scales          = 0x00800000,
        Tusks           = 0x01000000,
    }
}
