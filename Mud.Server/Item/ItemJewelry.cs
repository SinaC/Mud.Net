using System;
using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemJewelry : ItemEquipableBase<ItemJewelryBlueprint>, IItemJewelry
    {
        public ItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        // No additional datas
    }
}
