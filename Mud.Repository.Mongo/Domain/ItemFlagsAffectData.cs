namespace Mud.Repository.Mongo.Domain
{
    public class ItemFlagsAffectData : AffectDataBase
    {
        public int Operator { get; set; } // Add and Or are identical

        public int Modifier { get; set; }
    }
}
