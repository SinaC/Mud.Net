using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Item;

[DataContract]
public class ItemDrinkContainerBlueprint : ItemBlueprintBase
{
    [DataMember]
    public int MaxLiquidAmount { get; set; } // v0

    [DataMember]
    public int CurrentLiquidAmount { get; set; } // v1

    [DataMember]
    public string? LiquidType { get; set; } // v2

    [DataMember]
    public bool IsPoisoned { get; set; } // v3 0: normal A: poisoned
}
