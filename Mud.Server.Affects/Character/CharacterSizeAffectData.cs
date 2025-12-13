using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "size")]
public class CharacterSizeAffectData : AffectDataBase
{
    public required Sizes Value { get; set; }
}
