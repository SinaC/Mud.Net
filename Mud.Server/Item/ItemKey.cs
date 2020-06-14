using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemKey : ItemBase<ItemKeyBlueprint>, IItemKey
    {
        public ItemKey(Guid guid, ItemKeyBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemKey(Guid guid, ItemKeyBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
        }

        // No additional datas
    }
}
