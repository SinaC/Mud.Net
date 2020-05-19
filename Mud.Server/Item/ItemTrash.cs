using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemTrash : ItemBase<ItemTrashBlueprint, ItemData>, IItemTrash
    {
        public ItemTrash(Guid guid, ItemTrashBlueprint blueprint, IContainer containedInto) : base(guid, blueprint, containedInto)
        {
        }

        public ItemTrash(Guid guid, ItemTrashBlueprint blueprint, ItemData data, IContainer containedInto) : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
