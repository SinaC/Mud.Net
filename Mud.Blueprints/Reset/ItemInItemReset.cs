namespace Mud.Blueprints.Reset;

public class ItemInItemReset : ResetBase // 'P'
{
    public int ItemId { get; set; } // arg1
    public int ContainerId { get; set; } // arg3
    public int GlobalLimit { get; set; } // arg2
    public int LocalLimit { get; set; } // arg4
}
