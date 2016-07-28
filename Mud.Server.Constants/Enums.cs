namespace Mud.Server.Constants
{
    public enum Sex
    {
        Neutral,
        Male,
        Female
    }

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
        Wield2,  // mutually exclusive with Hold, Shield
        Hold, // mutually exclusive with Wield2, Shield
        Shield, // mutually exclusive with Wield2, Hold
        Wield2H // wield 2-handed
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

    public enum ComputedAttributeTypes // dependent on primary attribute, aura and items (computed in Recompute)
    {
        MaxHitPoints,
        AttackSpeed,
        AttackPower,
        SpellPower,
        Armor,
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
        Absorb,
        Armor,
    }
}
