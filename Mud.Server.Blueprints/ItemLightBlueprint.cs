namespace Mud.Server.Blueprints
{
    public class ItemLightBlueprint : ItemBlueprintBase
    {
        public int DurationHours { get; set; } // in hours, -1: means infinite
    }
}
