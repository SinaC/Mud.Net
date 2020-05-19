namespace Mud.Server.Blueprints.Reset
{
    public class ItemInRoomReset : ResetBase // 'O'
    {
        public int ItemId { get; set; } // arg1
        public int GlobalLimit { get; set; } // arg2
        public int LocalLimit { get; set; } // arg4
    }
}
