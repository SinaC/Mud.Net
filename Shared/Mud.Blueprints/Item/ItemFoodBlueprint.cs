namespace Mud.Blueprints.Item;

public class ItemFoodBlueprint : ItemBlueprintBase
{
    public int FullHours { get; set; }

    public int HungerHours { get; set; }

    public bool IsPoisoned { get; set; }
}
