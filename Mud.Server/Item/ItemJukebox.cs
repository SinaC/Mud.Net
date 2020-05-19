using Mud.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.Server.Item
{
    public class ItemJukebox : ItemBase<ItemJukeboxBlueprint, ItemData>, IItemJukebox
    {
        public ItemJukebox(Guid guid, ItemJukeboxBlueprint blueprint, IContainer containedInto) : base(guid, blueprint, containedInto)
        {
        }

        public ItemJukebox(Guid guid, ItemJukeboxBlueprint blueprint, ItemData data, IContainer containedInto) : base(guid, blueprint, data, containedInto)
        {
        }
    }
}
