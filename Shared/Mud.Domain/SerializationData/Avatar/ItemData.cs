namespace Mud.Domain.SerializationData.Avatar;

public class ItemData
{
    public required int ItemId { get; set; }

    public required string Source { get; set; }

    public required string ShortDescription { get; set; }

    public required string Description { get; set; }

    public required int Level { get; set; } // can be randomized during creation if RandomStats is set

    public required int Cost { get; set; } // can be randomized during creation if RandomStats is set

    public required int DecayPulseLeft { get; set; }

    public required string ItemFlags { get; set; }

    public required AuraData[] Auras { get; set; }
}
