using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemFood : ItemBase<ItemFoodBlueprint, ItemFoodData>, IItemFood
    {
        public ItemFood(Guid guid, ItemFoodBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            FullHours = blueprint.FullHours;
            HungerHours = blueprint.HungerHours;
            IsPoisoned = blueprint.IsPoisoned;
        }

        public ItemFood(Guid guid, ItemFoodBlueprint blueprint, ItemFoodData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            FullHours = data.FullHours;
            HungerHours = data.HungerHours;
            IsPoisoned = data.IsPoisoned;
        }

        #region IItemFood

        #region IItemPoisonable

        public bool IsPoisoned { get; protected set; }

        public void Poison()
        {
            IsPoisoned = true;
        }

        public void Cure()
        {
            IsPoisoned = false;
        }

        #endregion

        public int FullHours { get; protected set; }

        public int HungerHours { get; protected set; }

        public void SetHours(int fullHours, int hungerHours) // TODO: should be replaced with ctor parameters
        {
            FullHours = fullHours;
            HungerHours = hungerHours;
        }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemFoodData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags, // Current will be recompute with auras
                Auras = MapAuraData(),
                FullHours = FullHours,
                HungerHours = HungerHours,
                IsPoisoned = IsPoisoned
            };
        }

        #endregion
    }
}
