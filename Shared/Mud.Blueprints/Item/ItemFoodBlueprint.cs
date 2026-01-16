using System.Runtime.Serialization;

namespace Mud.Blueprints.Item;

[DataContract]
public class ItemFoodBlueprint : ItemBlueprintBase
{
    [DataMember]
    public int FullHours { get; set; }

    [DataMember]
    public int HungerHours { get; set; }

    [DataMember]
    public bool IsPoisoned { get; set; }
}
