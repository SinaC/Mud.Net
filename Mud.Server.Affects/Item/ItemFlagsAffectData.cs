using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Affects.Item;

[JsonPolymorphism(typeof(AffectDataBase), "itemFlags")]
public class ItemFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IItemFlags Modifier { get; set; }
}
