namespace Mud.Server.Blueprints.Reset
{
    public class ItemInItemReset : ResetBase
    {
        public int ItemId { get; set; }
        public int ContainerId { get; set; }
        public int LoadCount { get; set; }
        public int GlobalLimit { get; set; }
        public int LocalLimit { get; set; }
    }
}
