using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item
{
    public class ItemScroll : ItemCastSpellsNoChargeBase<ItemScrollBlueprint>, IItemScroll
    {
        public ItemScroll(Guid guid, ItemScrollBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemScroll(Guid guid, ItemScrollBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
