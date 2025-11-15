using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Item;

[DataContract]
public class ItemFountainBlueprint : ItemBlueprintBase
{
    [DataMember]
    public string LiquidType { get; set; } = default!; // v2
}
