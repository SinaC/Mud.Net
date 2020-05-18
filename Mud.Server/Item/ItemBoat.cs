using System;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemBoat : ItemBase<ItemBoatBlueprint, ItemData>, IItemBoat
    {
        public ItemBoat(Guid guid, ItemBoatBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
        }

        public ItemBoat(Guid guid, ItemBoatBlueprint blueprint, ItemData data, IContainer containedInto)
            : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
