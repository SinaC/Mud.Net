using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(AffectDataBase), "sex")]
public class CharacterSexAffectData : AffectDataBase
{
    public required Sex Value { get; set; }
}
