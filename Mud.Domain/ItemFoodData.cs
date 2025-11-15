namespace Mud.Domain;

public class ItemFoodData : ItemData
{
    public required int FullHours { get; set; }
    public required int HungerHours { get; set; }
    public required bool IsPoisoned { get; set; }
}
