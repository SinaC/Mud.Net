namespace Mud.Repository.Mongo.Domain
{
    public class EquippedItemData
    {
        public int Slot { get; set; }

        public ItemData Item { get; set; }
    }
}
