using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "characterAttribute")]
public class CharacterAttributeAffectData : AffectDataBase
{
    public AffectOperators Operator { get; set; } // Or and Nor cannot be used
    public CharacterAttributeAffectLocations Location { get; set; }
    public int Modifier { get; set; }
}
