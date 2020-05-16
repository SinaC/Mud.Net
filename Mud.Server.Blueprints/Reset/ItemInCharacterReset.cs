namespace Mud.Server.Blueprints.Reset
{
    public class ItemInCharacterReset : ResetBase
    {
        public int ItemId { get; set; }
        public int GlobalLimit { get; set; }
    }
}
