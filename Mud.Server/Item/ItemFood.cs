using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemFood : ItemBase<ItemFoodBlueprint>, IItemFood
    {
        public ItemFood(Guid guid, ItemFoodBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            FullHour = blueprint.FullHour;
            HungerHour = blueprint.HungerHour;
            IsPoisoned = blueprint.IsPoisoned;
        }

        public ItemFood(Guid guid, ItemFoodBlueprint blueprint, ItemFoodData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
            FullHour = blueprint.FullHour;
            HungerHour = blueprint.HungerHour;
            IsPoisoned = data.IsPoisoned;
        }

        #region IItemFood

        public int FullHour { get; }

        public int HungerHour { get; }

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
                IsPoisoned = IsPoisoned
            };
        }

        #endregion
    }
}
