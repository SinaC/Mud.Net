using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemStaff : ItemCastSpellsChargeBase<ItemStaffBlueprint, ItemStaffData>, IItemStaff
    {
        public ItemStaff(Guid guid, ItemStaffBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemStaff(Guid guid, ItemStaffBlueprint blueprint, ItemStaffData data, IContainer containedInto) 
            : base(guid, blueprint, data, containedInto)
        {
        }

        public override ItemData MapItemData()
        {
            return new ItemStaffData
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
    }
}
