using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

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
