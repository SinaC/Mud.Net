using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "corpse")]
public class ItemCorpseData : ItemData
{
    public required ItemData[] Contains { get; set; }

    public required bool IsPlayableCharacterCorpse { get; set; }

    public required string CorpseName { get; set; }

    public required bool HasBeenGeneratedByKillingCharacter { get; set; }
}
