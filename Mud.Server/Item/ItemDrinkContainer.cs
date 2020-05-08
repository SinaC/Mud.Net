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
            MaxLiquidAmount = blueprint.MaxLiquidAmount;
            CurrentLiquidAmount = blueprint.CurrentLiquidAmount;
            IsPoisoned = blueprint.IsPoisoned;
        }

        public ItemDrinkContainer(Guid guid, ItemDrinkContainerBlueprint blueprint, ItemDrinkContainerData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            LiquidName = data.LiquidName;
            MaxLiquidAmount = data.MaxLiquidAmount;
            CurrentLiquidAmount = data.CurrentLiquidAmount;
            IsPoisoned = data.IsPoisoned;
        }

        #region IItemDrinkContainer

        #region IItemDrinkable

        public string LiquidName { get; protected set; }

        public int LiquidLeft => CurrentLiquidAmount;

        public bool IsEmpty => CurrentLiquidAmount <= 0;

        public bool IsPoisoned { get; protected set; }

        public int LiquidAmountMultiplier => 1;

        public void Drink(int amount)
        {
            CurrentLiquidAmount = Math.Max(0, CurrentLiquidAmount - amount);
        }

        #endregion

        public int MaxLiquidAmount { get; protected set; }

        public int CurrentLiquidAmount { get; protected set; }

        public void Poison()
        {
            IsPoisoned = true;
        }

        public void Cure()
        {
            IsPoisoned = false;
        }

        public void Fill(string liquidName)
        {
            LiquidName = liquidName;
            CurrentLiquidAmount = MaxLiquidAmount;
        }

        public void Empty()
        {
            CurrentLiquidAmount = 0;
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
                MaxLiquidAmount = MaxLiquidAmount,
                CurrentLiquidAmount = CurrentLiquidAmount,
                LiquidName = LiquidName,
                IsPoisoned = IsPoisoned,
                Auras = MapAuraData(),
            };
        }

#endregion
    }
}
