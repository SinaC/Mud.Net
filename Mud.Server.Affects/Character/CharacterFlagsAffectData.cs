using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "characterFlags")]
public class CharacterFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
