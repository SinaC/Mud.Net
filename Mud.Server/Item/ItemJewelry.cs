using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemJewelry : ItemEquippableBase<ItemJewelryBlueprint>, IItemJewelry
    {
        public ItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
        }

        #region IItem

        public override int CarryCount => 0;

        #endregion

        // No additional datas
    }
}
