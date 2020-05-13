using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemPill : ItemCastSpellsNoChargeBase<ItemPillBlueprint>, IItemPill
    {
        public ItemPill(Guid guid, ItemPillBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemPill(Guid guid, ItemPillBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }
       
    }
}
