using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemWand : ItemCastSpellsChargeBase<ItemWandBlueprint, ItemWandData>, IItemWand
    {
        public ItemWand(Guid guid, ItemWandBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemWand(Guid guid, ItemWandBlueprint blueprint, ItemWandData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemWandData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                Auras = MapAuraData(),
                MaxChargeCount = MaxChargeCount,
                CurrentChargeCount = CurrentChargeCount,
                AlreadyRecharged = AlreadyRecharged
            };
        }

        #endregion

    }
}
