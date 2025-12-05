using Mud.Server.Flags.Interfaces;

namespace Mud.Domain.SerializationData;

public class ItemWeaponData : ItemData
{
    public required IWeaponFlags WeaponFlags { get; set; }
}
