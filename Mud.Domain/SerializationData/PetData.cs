using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonBaseType(typeof(CharacterData))]
public class PetData : CharacterData
{
    public required int BlueprintId { get; set; }
}
