﻿namespace Mud.Domain
{
    public class ItemPortalData : ItemData
    {
        public int DestinationRoomId { get; set; }
        public PortalFlags PortalFlags { get; set; }
        public int MaxChargeCount { get; set; }
        public int CurrentChargeCount { get; set; }
    }
}
