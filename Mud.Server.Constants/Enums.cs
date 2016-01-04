namespace Mud.Server.Constants
{
    public enum Sex
    {
        Neutral,
        Male,
        Female
    }

    public enum Parts
    {
        Head,
        Arm,
        Leg,
        Body,
        Hand,
        Foot,
        Finger,
        Ear,
        Eye,
        Heart,
        Brains,
        Guts,
        LongTongue,
        EyeStalks,
        Tentacles,
        Fins,
        Wings,
        Tail,
        Claws,
        Fangs,
        Horns,
        Scales,
        Tusks
    }

    public enum WearLocations
    {
        None,
        Light, // one
        Head, // one per head
        Eyes, // one by head
        Ear, // one by ear
        Neck, // one by head
        Arms, // one by 1-3-5-... arms
        Hands, // one by 1-3-5-... arms
        Wrist, // one by arm
        Finger, // one by arm
        Wield, // one by mainhand
        Offhand, // one by offhand
        Body, // one by torso
        About, // one by torso
        Waist, // one by torso
        Legs, // one by 1-3-5-... legs
        Feet, // one by 1-3-5-... legs
        Float // no part needed
    }

    public enum WeaponTypes
    {
        Exotic,
        Sword,
        Dagger,
        Spear,
        Mace,
        Axe,
        Flail,
        Whip,
        Polearm,
        Staff
    }

    public enum DamageTypes
    {
        Bash,
        Pierce,
        Slash,
        Fire,
        Cold,
        Lightning,
        Acid,
        Poison,
        Negative,
        Holy,
        Energy,
        Mental,
        Disease,
        Drowning,
        Light,
        Earth,
    }
}
