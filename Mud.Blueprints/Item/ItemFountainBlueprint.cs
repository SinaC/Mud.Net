using System.Runtime.Serialization;

namespace Mud.Blueprints.Item;

[DataContract]
public class ItemFountainBlueprint : ItemBlueprintBase
{
    [DataMember]
    public string? LiquidType { get; set; } // v2
}
