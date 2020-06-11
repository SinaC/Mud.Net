using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public class ItemGem : ItemBase<ItemGemBlueprint, ItemData>, IItemGem
    {
        public ItemGem(Guid guid, ItemGemBlueprint blueprint, IContainer containedInto) : base(guid, blueprint, containedInto)
        {
        }

        public ItemGem(Guid guid, ItemGemBlueprint blueprint, ItemData data, IContainer containedInto) : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
