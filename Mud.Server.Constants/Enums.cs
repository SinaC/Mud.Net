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
        Shield // mutually exclusive with Wield2, Hold
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

    public enum AttributeTypes
    {
        Strength,
        Agility,
        Stamina,
        Intellect,
        Spirit
    }

    public enum ResourceKinds
    {
        None,
        Mana,
        Energy,
        Rage
    }

    public enum AmountOperators
    {
        None,
        Fixed,
        Percentage
    }
}
