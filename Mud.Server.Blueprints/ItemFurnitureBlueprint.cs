namespace Mud.Server.Blueprints
{
    public class ItemFurnitureBlueprint : ItemBlueprintBase
    {
        public int MaxPeople { get; set; }
        public int MaxWeight { get; set; }
        // TODO: flags (see tables.C furniture_flags)
        public int HealBonus { get; set; }
        public int ResourceBonus { get; set; }
    }
}
