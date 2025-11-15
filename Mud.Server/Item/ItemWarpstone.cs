using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public class ItemWarpstone : ItemBase<ItemWarpStoneBlueprint, ItemData>, IItemWarpstone
{
    public ItemWarpstone(Guid guid, ItemWarpStoneBlueprint blueprint, IContainer containedInto)
        : base(guid, blueprint, containedInto)
    {
    }

    public ItemWarpstone(Guid guid, ItemWarpStoneBlueprint blueprint, ItemData data, IContainer containedInto) 
        : base(guid, blueprint, data, containedInto)
    {
    }
}
