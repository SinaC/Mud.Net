using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemTreasure : ItemBase<ItemTreasureBlueprint, ItemData>, IItemTreasure
    {
        public ItemTreasure(Guid guid, ItemTreasureBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemTreasure(Guid guid, ItemTreasureBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
