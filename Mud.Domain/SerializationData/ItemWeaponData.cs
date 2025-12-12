using Mud.Domain.Serialization;
using Mud.Server.Flags.Interfaces;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "weapon")]
public class ItemWeaponData : ItemData
{
    public required IWeaponFlags WeaponFlags { get; set; }
}
