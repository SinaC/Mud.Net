using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemWarpstone : ItemBase<ItemWarpstoneBlueprint, ItemData>, IItemWarpstone
    {
        public ItemWarpstone(Guid guid, ItemWarpstoneBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemWarpstone(Guid guid, ItemWarpstoneBlueprint blueprint, ItemData data, IContainer containedInto) 
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
