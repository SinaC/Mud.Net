namespace Mud.Repository.Filesystem.Domain;

public class ItemContainerData : ItemData
{
    public int MaxWeight { get; set; }

    public int ContainerFlags { get; set; }

    public int MaxWeightPerItem { get; set; }

    public ItemData[] Contains { get; set; }
}
