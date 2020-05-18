using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
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
}
