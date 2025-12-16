using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonBaseType(typeof(AffectDataBase), "irv")]
public class CharacterIRVAffectData : AffectDataBase
{
    public required IRVAffectLocations Location { get; set; }

    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
