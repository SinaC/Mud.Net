using Mud.Domain;

namespace Mud.Server.Blueprints.Item;

public class ItemFurnitureBlueprint : ItemBlueprintBase
{
    public int MaxPeople { get; set; }
    public int MaxWeight { get; set; }
    public FurnitureActions FurnitureActions { get; set; }
    public FurniturePlacePrepositions FurniturePlacePreposition { get; set; }
    public int HealBonus { get; set; }
    public int ResourceBonus { get; set; }
}
