using Mud.Server.Flags.Interfaces;
using System.Text.Json.Serialization;

namespace Mud.Domain.SerializationData;

public abstract class CharacterData
{
    public required string Name { get; set; }

    public required string Race { get; set; }

    public required string Class { get; set; }

    public required int Level { get; set; }

    public required Sex Sex { get; set; }

    public required Sizes Size { get; set; }

    public required int HitPoints { get; set; }

    public required int MovePoints { get; set; }

    public required Dictionary<ResourceKinds, int> CurrentResources { get; set; }

    public required Dictionary<ResourceKinds, int> MaxResources { get; set; }

    public required EquippedItemData[] Equipments { get; set; }

    public required ItemData[] Inventory { get; set; }

    public required AuraData[] Auras { get; set; }

    public required ICharacterFlags CharacterFlags { get; set; }

    public required IIRVFlags Immunities { get; set; }

    public required IIRVFlags Resistances { get; set; }

    public required IIRVFlags Vulnerabilities { get; set; }
    public required IShieldFlags ShieldFlags { get; set; }

    public required Dictionary<CharacterAttributes, int> Attributes { get; set; }
}
