namespace Mud.Repository.Mongo.Domain
{
    public class ItemPortalData : ItemData
    {
        public int DestinationRoomId { get; set; }

        public int PortalFlags { get; set; }

        public int MaxChargeCount { get; set; }

        public int CurrentChargeCount { get; set; }
    }
}
