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
        NoRemove        = 0x00001000, // Cannot be removed once equiped [can be uncursed]
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
        Bash = 1,
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

    public enum DispelTypes
    {
        None        = 0,
        Magic       = 1,
        Poison      = 2,
        Disease     = 3,
        Curse       = 4,
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

    public enum CharacterAttributes // must be ordered, starts at 0 and can't contain holes
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
        ArmorMagic      = 13
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
        // not used
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
        // GodsOnly
        // HeroesOnly
        // NewbiesOnly
        Law         = 0x00040000,
        Nowhere     = 0x00080000,
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
        None = 0,
        Fixed = 1,
        Percentage = 2
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
        Noalign         = 0x00000080,
        Nopurge         = 0x00000100,
        Outdoors        = 0x00000200,
        Indoors         = 0x00000400,
        UpdateAlways    = 0x00000800,
        Train           = 0x00001000,
        IsHealer        = 0x00002000,
        Gain            = 0x00004000,
        Practice        = 0x00008000,
        Aware           = 0x00010000,
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

    [Flags]
    public enum AbilityFlags
    {
        None                = 0x00000000,
        AuraIsHidden        = 0x00000001,
        CannotMiss          = 0x00000002,
        CannotBeReflected   = 0x00000004,
        CannotBeUsed        = 0x00000008,
    }

    public enum AbilityKinds
    {
        Passive = 0,
        Spell   = 1, // invoked with cast
        Skill   = 2, // invoked with use or command
    }

    public enum AbilityTargets
    {
        // No target
        None                                = 0,
        // Fighting if no parameter, character in room if parameter specified
        CharacterOffensive                  = 1,
        // Itself if no parameter, character in room if parameter specified
        CharacterDefensive                  = 2,
        // Itself if no parameter, check if parameter == itself if parameter specified
        CharacterSelf                       = 3,
        // Item in inventory
        ItemInventory                       = 4,
        // Fighting if no parameter, character in room, then item in room, then in inventory, then in equipment if parameter specified
        ItemHereOrCharacterOffensive        = 5,
        // Itself if no parameter, character in room or item in inventory if parameter specified
        ItemInventoryOrCharacterDefensive   = 6,
        // Target will be 'computed' by spell
        Custom                              = 7,
        // Optional item in inventory
        OptionalItemInventory               = 8,
        // Armor in inventory
        ArmorInventory                      = 9,
        // Weapon in inventory
        WeaponInventory                     = 10,
        // Victim is source.Fighting
        CharacterFighting                   = 11,
        // Victim is somewhere in the world
        CharacterWorldwide                  = 12,
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
        Resets    = 0x00001000, // TODO: use
        Restore   = 0x00002000,
    }
}
