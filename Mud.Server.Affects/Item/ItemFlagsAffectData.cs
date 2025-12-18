using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Item;

[JsonBaseType(typeof(AffectDataBase), "itemFlags")]
public class ItemFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
