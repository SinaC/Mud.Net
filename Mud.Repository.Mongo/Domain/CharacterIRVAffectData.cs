namespace Mud.Repository.Mongo.Domain
{
    public class CharacterIRVAffectData : AffectDataBase
    {
        public int Location { get; set; }

        public int Operator { get; set; } // Add and Or are identical

        public int Modifier { get; set; }
    }
}
