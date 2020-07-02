using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class CharacterIRVAffectData : AffectDataBase
    {
        public IRVAffectLocations Location { get; set; }

        public AffectOperators Operator { get; set; } // Add and Or are identical

        public IIRVFlags Modifier { get; set; }
    }
}
