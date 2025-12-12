using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonPolymorphism(typeof(ItemData), "container")]
public class ItemContainerData : ItemData
{
    public required int MaxWeight { get; set; }
    public required ContainerFlags ContainerFlags { get; set; }
    public required int MaxWeightPerItem { get; set; }
    public required ItemData[] Contains { get; set; }
}
