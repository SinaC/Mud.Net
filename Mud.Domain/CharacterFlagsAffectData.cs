using Mud.DataStructures.Flags;

namespace Mud.Domain
{
    public class CharacterFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public Flags Modifier { get; set; }
    }
}
