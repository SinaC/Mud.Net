namespace Mud.Repository.Filesystem.Domain
{
    public class CharacterAttributeAffectData : AffectDataBase
    {
        public int Operator { get; set; } // Or and Nor cannot be used

        public int Location { get; set; }

        public int Modifier { get; set; }
    }
}
