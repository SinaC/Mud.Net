using System;

namespace Mud.Domain
{
    public enum Sex
    {
        Neutral,
        Male,
        Female
    }

    public enum Positions
    {
        Stunned,
        Sleeping,
        Resting,
        Sitting,
        Fighting,
        Standing
    }

    [Flags]
    public enum FurnitureActions
    {
        None  = 0x0000,
        Stand = 0x0001,
        Sit   = 0x0002,
        Rest  = 0x0004,
        Sleep = 0x0008
    }

    public enum FurniturePlacePrepositions
    {
        None = 0,
        At,
        In,
        On
    }

    public enum Forms
    {
        Normal,
        Bear, // druid
        Cat, // druid
        Travel, // druid
        Moonkin, // druid
        Tree, // druid
        Shadow // priest
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

    public enum WearLocations
    {
        None,
        Light,
        Head,
        Amulet,
        Shoulders,
        Chest,
        Cloak,
        Waist,
        Wrists,
        Arms,
        Hands,
        Ring,
        Legs,
        Feet,
        Trinket,
        Wield,
        Hold,
        Shield,
        Wield2H
    }

    public enum EquipmentSlots
    {
        None,
        Light,
        Head,
        Amulet,
        Shoulders,
        Chest,
        Cloak,
        Waist,
        Wrists,
        Arms,
        Hands,
        RingLeft,
        RingRight,
        Legs,
        Feet,
        Trinket1,
        Trinket2,
        Wield,
        Wield2, // mutually exclusive with Hold, Shield
        Hold, // mutually exclusive with Wield2, Shield
        Shield, // mutually exclusive with Wield2, Hold
        Wield2H, // wield 2-handed
        Wield3,
        Wield4,
        Wield2H2,
    }

    public enum ArmorKinds
    {
        Cloth,
        Leather,
        Mail,
        Plate
    }

    public enum WeaponTypes
    {
        // one-handed
        Dagger,
        Fist,
        Axe1H,
        Mace1H,
        Sword1H,
        // two-handed
        Polearm,
        Stave,
        Axe2H,
        Mace2H,
        Sword2H
    }

    public enum SchoolTypes
    {
        None,
        Physical,
        Arcane,
        Fire,
        Frost,
        Nature,
        Shadow,
        Holy,
    }

    public enum DispelTypes
    {
        None,
        Magic,
        Poison,
        Disease,
        Curse,
    }

    public enum PrimaryAttributeTypes
    {
        Strength,
        Agility,
        Stamina,
        Intellect,
        Spirit
    }

    public enum SecondaryAttributeTypes // dependent on primary attribute, aura and items (computed in Recompute)
    {
        MaxHitPoints,
        AttackSpeed,
        AttackPower,
        SpellPower,
        Armor,
        Critical,
        Dodge,
        Parry,
        Block
    }

    public enum ResourceKinds
    {
        None,
        Mana,
        Energy,
        Rage,
        Runic,
        // TODO: runes
        //BloodRune,
        //FrostRune,
        //UnholyRune,
        //DeathRune
    }

    public enum AmountOperators
    {
        None,
        Fixed,
        Percentage
    }

    public enum AuraModifiers
    {
        None,
        Strength,
        Agility,
        Stamina,
        Intellect,
        Spirit,
        Characteristics,
        AttackSpeed,
        AttackPower,
        SpellPower,
        MaxHitPoints,
        DamageAbsorb,
        HealAbsorb,
        Armor,
        Critical,
        Dodge,
        Parry,
        Block
    }

    public enum AdminLevels
    {
        Angel,
        DemiGod,
        Immortal,
        God,
        Deity,
        Supremacy,
        Creator,
        Implementor,
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
