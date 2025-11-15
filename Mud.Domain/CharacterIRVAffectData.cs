using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class CharacterIRVAffectData : AffectDataBase
{
    public required IRVAffectLocations Location { get; set; }

    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IIRVFlags Modifier { get; set; }
}
