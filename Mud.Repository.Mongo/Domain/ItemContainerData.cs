namespace Mud.Repository.Mongo.Domain
{
    public class ItemContainerData : ItemData
    {
        public int ContainerFlags { get; set; }

        public ItemData[] Contains { get; set; }
    }
}
