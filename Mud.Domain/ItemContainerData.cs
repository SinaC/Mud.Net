namespace Mud.Domain
{
    public class ItemContainerData : ItemData
    {
        public ContainerFlags ContainerFlags { get; set; }
        public ItemData[] Contains { get; set; }
    }
}
