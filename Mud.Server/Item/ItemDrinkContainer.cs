using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemDrinkContainer : ItemBase<ItemDrinkContainerBlueprint>, IItemDrinkContainer
    {
        public ItemDrinkContainer(Guid guid, ItemDrinkContainerBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            LiquidName = blueprint.LiquidType;
            MaxLiquid = blueprint.MaxLiquidAmount;
            LiquidLeft = blueprint.CurrentLiquidAmount;
            IsPoisoned = blueprint.IsPoisoned;
        }

        public ItemDrinkContainer(Guid guid, ItemDrinkContainerBlueprint blueprint, ItemDrinkContainerData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            LiquidName = data.LiquidName;
            MaxLiquid = data.MaxLiquidAmount;
            LiquidLeft = data.CurrentLiquidAmount;
            IsPoisoned = data.IsPoisoned;
        }

        #region IItemDrinkContainer

        #region IItemDrinkable

        public string LiquidName { get; protected set; }

        public int LiquidLeft { get; protected set; }

        public bool IsEmpty => LiquidLeft <= 0;

        public bool IsPoisoned { get; protected set; }

        public int LiquidAmountMultiplier => 1;

        public void Drink(int amount)
        {
            LiquidLeft = Math.Max(0, LiquidLeft - amount);
        }

        #endregion

        public int MaxLiquid { get; protected set; }

        public void Poison()
        {
            IsPoisoned = true;
        }

        public void Cure()
        {
            IsPoisoned = false;
        }

        public void Fill(string liquidName, int amount)
        {
            LiquidName = liquidName;
            LiquidLeft = Math.Min(MaxLiquid, LiquidLeft + amount);
        }

        public void Fill(int amount)
        {
            LiquidLeft = Math.Min(MaxLiquid, LiquidLeft + amount);
        }

        public void Pour()
        {
            LiquidLeft = 0;
            IsPoisoned = false;
        }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemDrinkContainerData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                MaxLiquidAmount = MaxLiquid,
                CurrentLiquidAmount = LiquidLeft,
                LiquidName = LiquidName,
                IsPoisoned = IsPoisoned,
                Auras = MapAuraData(),
            };
        }

#endregion
    }
}
