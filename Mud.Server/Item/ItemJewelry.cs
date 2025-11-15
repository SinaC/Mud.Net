using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public class ItemJewelry : ItemBase<ItemJewelryBlueprint, ItemData>, IItemJewelry
{
    public ItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer containedInto) 
        : base(guid, blueprint, containedInto)
    {
    }

    public ItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, ItemData itemData, IContainer containedInto)
        : base(guid, blueprint, itemData, containedInto)
    {
    }

    #region IItem

    public override int CarryCount => 0;

    #endregion

    // No additional datas
}
