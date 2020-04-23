using System;
using Mud.Domain;
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

        public ItemPortal(Guid guid, ItemPortalBlueprint blueprint, ItemData itemData, IRoom destination, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Destination = destination;
        }

        #region IItemPortal

        public IRoom Destination { get; }

        #endregion
    }
}
