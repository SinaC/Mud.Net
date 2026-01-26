using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item;

public class ItemFurnitureBlueprint : ItemBlueprintBase
{
    public int MaxPeople { get; set; }
    public int MaxWeight { get; set; }
    public IFurnitureActions FurnitureActions { get; set; } = default!;
    public FurniturePlacePrepositions FurniturePlacePreposition { get; set; }
    public int HealBonus { get; set; }
    public int ResourceBonus { get; set; }
}
