namespace Mud.Domain
{
    public class CharacterAttributeAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Or and Nor cannot be used
        public CharacterAttributeAffectLocations Location { get; set; }
        public int Modifier { get; set; }
    }
}
