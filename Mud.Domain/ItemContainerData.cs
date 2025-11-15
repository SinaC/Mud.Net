namespace Mud.Domain;

public class ItemContainerData : ItemData
{
    public required int MaxWeight { get; set; }
    public required ContainerFlags ContainerFlags { get; set; }
    public required int MaxWeightPerItem { get; set; }
    public required ItemData[] Contains { get; set; }
}
