using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character;

[JsonPolymorphism(typeof(AffectDataBase), "sex")]
public class CharacterSexAffectData : AffectDataBase
{
    public required Sex Value { get; set; }
}
