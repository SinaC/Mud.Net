using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemDrinkContainer : IItemDrinkable, IItemPoisonable
{
    void Initialize(Guid guid, ItemDrinkContainerBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemDrinkContainerBlueprint blueprint, ItemDrinkContainerData data, IContainer containedInto);

    int MaxLiquid { get; }

    void Fill(string liquidName, int amount);
    void Fill(int amount);
    void Pour();
}
