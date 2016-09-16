using System;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemKey : ItemBase<ItemKeyBlueprint>, IItemKey
    {
        public ItemKey(Guid guid, ItemKeyBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }
    }
}
