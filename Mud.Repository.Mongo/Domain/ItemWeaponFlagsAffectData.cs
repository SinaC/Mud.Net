namespace Mud.Repository.Mongo.Domain
{
    public class ItemWeaponFlagsAffectData : AffectDataBase
    {
        public int Operator { get; set; } // Add and Or are identical

        public string Modifier { get; set; }
    }
}
