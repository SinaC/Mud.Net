using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class CharacterFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public ICharacterFlags Modifier { get; set; }
    }
}
