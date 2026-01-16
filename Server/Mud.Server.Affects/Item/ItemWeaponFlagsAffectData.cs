using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Item;

[JsonBaseType(typeof(AffectDataBase), "weaponFlags")]
public class ItemWeaponFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
