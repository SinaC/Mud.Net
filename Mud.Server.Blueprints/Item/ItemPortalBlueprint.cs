using Mud.Domain;

namespace Mud.Server.Blueprints.Item
{
    public class ItemPortalBlueprint : ItemBlueprintBase
    {
        public int Destination { get; set; }
        public int Key { get; set; }
        public PortalFlags PortalFlags { get; set; }
        public int MaxChargeCount { get; set; } // -1: infinite
        public int CurrentChargeCount { get; set; }
    }
}
