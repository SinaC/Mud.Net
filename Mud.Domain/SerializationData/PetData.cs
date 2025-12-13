using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(CharacterData))]
public class PetData : CharacterData
{
    public required int BlueprintId { get; set; }
}
