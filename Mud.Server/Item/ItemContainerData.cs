using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "container")]
public class ItemContainerData : ItemData
{
    public required int MaxWeight { get; set; }
    public required ContainerFlags ContainerFlags { get; set; }
    public required int MaxWeightPerItem { get; set; }
    public required ItemData[] Contains { get; set; }
}
