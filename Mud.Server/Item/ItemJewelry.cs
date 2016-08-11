using System;
using Mud.Server.Blueprints;

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
