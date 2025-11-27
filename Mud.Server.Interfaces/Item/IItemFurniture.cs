using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using System.Text;

namespace Mud.Server.Interfaces.Item;

public interface IItemFurniture : IItem // TODO: count people actually on furniture
{
    void Initialize(Guid guid, ItemFurnitureBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemFurnitureBlueprint blueprint, ItemData itemData, IContainer containedInto);

    IEnumerable<ICharacter>? People { get; }

    int MaxPeople { get; }
    int MaxWeight { get; }
    FurnitureActions FurnitureActions { get; }
    FurniturePlacePrepositions FurniturePlacePreposition { get; }
    int HealBonus { get; }
    int ResourceBonus { get; }

    bool CanStand { get; }
    bool CanSit { get; }
    bool CanRest { get; }
    bool CanSleep { get; }

    StringBuilder AppendPosition(StringBuilder sb, string verb);
}
