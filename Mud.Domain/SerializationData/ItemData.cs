using Mud.Domain.Serialization;
using Mud.Server.Flags.Interfaces;

namespace Mud.Domain.SerializationData;

//[JsonPolymorphism(typeof(ItemData), "base")]
public class ItemData
{
    public required int ItemId { get; set; }

    public required int Level { get; set; }

    public required int DecayPulseLeft { get; set; }

    public required IItemFlags ItemFlags { get; set; }

    public required AuraData[] Auras { get; set; }
}
