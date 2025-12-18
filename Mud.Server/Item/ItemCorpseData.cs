using Mud.Common.Attributes;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "corpse")]
public class ItemCorpseData : ItemData
{
    public required ItemData[] Contains { get; set; }

    public required bool IsPlayableCharacterCorpse { get; set; }

    public required string CorpseName { get; set; }

    public required bool HasBeenGeneratedByKillingCharacter { get; set; }
}
