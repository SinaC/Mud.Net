namespace Mud.Server.Blueprints.Reset
{
    public class ItemInRoomReset : ResetBase
    {
        public int ItemId { get; set; }
        public int GlobalLimit { get; set; }
        public int LocalLimit { get; set; }
    }
}
