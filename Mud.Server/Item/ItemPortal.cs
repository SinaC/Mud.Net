using System;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemPortal : ItemBase<ItemPortalBlueprint>, IItemPortal
    {
        public ItemPortal(Guid guid, ItemPortalBlueprint blueprint, IRoom destination, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Destination = destination;
        }

        public IRoom Destination { get; }
    }
}
