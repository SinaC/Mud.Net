using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public class ItemClothing : ItemBase<ItemClothingBlueprint, ItemData>, IItemClothing
{
    public ItemClothing(Guid guid, ItemClothingBlueprint blueprint, IContainer containedInto)
        : base(guid, blueprint, containedInto)
    {
    }

    public ItemClothing(Guid guid, ItemClothingBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(guid, blueprint, data, containedInto)
    {
    }
}
