using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemFountain: IItemDrinkable
{
    void Initialize(Guid guid, ItemFountainBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemFountainBlueprint blueprint, ItemData data, IContainer containedInto);
}
