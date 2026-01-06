using Mud.Common.Attributes;

namespace Mud.Domain.SerializationData;

[JsonBaseType(typeof(CharacterData))]
public class PetData : CharacterData
{
    public required int BlueprintId { get; set; }
    public required string Description { get; set; }
}
