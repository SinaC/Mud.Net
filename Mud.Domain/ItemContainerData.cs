namespace Mud.Domain
{
    public class ItemContainerData : ItemData
    {
        public int MaxWeight { get; set; }
        public ContainerFlags ContainerFlags { get; set; }
        public int MaxWeightPerItem { get; set; }
        public ItemData[] Contains { get; set; }
    }
}
