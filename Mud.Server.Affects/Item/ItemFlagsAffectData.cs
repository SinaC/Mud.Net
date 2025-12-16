using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Item;

[JsonBaseType(typeof(AffectDataBase), "itemFlags")]
public class ItemFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
