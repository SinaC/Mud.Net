namespace Mud.Repository.Mongo.Domain
{
    public class CharacterFlagsAffectData : AffectDataBase
    {
        public int Operator { get; set; } // Add and Or are identical

        public int Modifier { get; set; }
    }
}
