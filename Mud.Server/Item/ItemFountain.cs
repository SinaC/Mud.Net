using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public class ItemFountain : ItemBase<ItemFountainBlueprint, ItemData>, IItemFountain
    {
        public ItemFountain(Guid guid, ItemFountainBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            LiquidName = blueprint.LiquidType;
        }

        public ItemFountain(Guid guid, ItemFountainBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            LiquidName = blueprint.LiquidType;
        }

        #region IItemFountain

        #region IItemDrinkable

        public string LiquidName { get; protected set; }

        public int LiquidLeft => int.MaxValue;

        public bool IsEmpty => false;

        public int LiquidAmountMultiplier => 3;

        public void Drink(int amount)
        {
            // NOP: infinite container
        }

        #endregion

        #endregion
    }
}
