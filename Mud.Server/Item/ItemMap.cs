using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemMap : ItemBase<ItemMapBlueprint, ItemData>, IItemMap
    {
        public ItemMap(Guid guid, ItemMapBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemMap(Guid guid, ItemMapBlueprint blueprint, ItemData data, IContainer containedInto) 
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
