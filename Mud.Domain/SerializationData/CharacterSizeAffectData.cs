using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(AffectDataBase), "size")]
public class CharacterSizeAffectData : AffectDataBase
{
    public required Sizes Value { get; set; }
}
