namespace Mud.Server.Blueprints.Reset;

public class ItemInCharacterReset : ResetBase // 'G'
{
    public int ItemId { get; set; } // arg1
    public int GlobalLimit { get; set; } // arg2
}
