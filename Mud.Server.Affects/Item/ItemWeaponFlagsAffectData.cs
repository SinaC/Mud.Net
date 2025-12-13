using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Affects.Item;

[JsonPolymorphism(typeof(AffectDataBase), "weaponFlags")]
public class ItemWeaponFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IWeaponFlags Modifier { get; set; }
}
