using System;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemFurniture : ItemBase<ItemFurnitureBlueprint>, IItemFurniture
    {
        public ItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer containedInto) : base(guid, blueprint, containedInto)
        {
            MaxPeople = blueprint.MaxPeople;
            MaxWeight = blueprint.MaxWeight;
            HealBonus = blueprint.HealBonus;
            ResourceBonus = blueprint.ResourceBonus;
        }

        #region IItemFurniture

        public int MaxPeople { get; }
        public int MaxWeight { get; }
        public int HealBonus { get; }
        public int ResourceBonus { get; }

        #endregion
    }
}
