using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "weapon")]
public class ItemWeaponData : ItemData
{
    public required string WeaponFlags { get; set; }
    public required int DiceCount { get; set; } // can be randomized during creation if RandomStats is set
    public required int DiceValue { get; set; } // can be randomized during creation if RandomStats is set
}
