using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public class ItemPotion : ItemCastSpellsNoChargeBase<ItemPotionBlueprint>, IItemPotion
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
