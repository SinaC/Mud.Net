using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemPotion : ItemCastSpellsNoRechargeBase<ItemPotionBlueprint>, IItemPotion
    {
        public ItemPotion(Guid guid, ItemPotionBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemPotion(Guid guid, ItemPotionBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
