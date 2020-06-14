namespace Mud.Domain
{
    public class ItemWeaponFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public WeaponFlags Modifier { get; set; }
    }
}
