using Mud.POC.Abilities2.Domain;
using Mud.Server.Blueprints.Item;
using System;
using Mud.POC.Abilities2.ExistingCode;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public interface IItemManager
    {
        IEnumerable<IItem> Items { get; }

        IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
        IItem AddItem(Guid guid, ItemData itemData, IContainer container);
        IItem AddItem(Guid guid, int blueprintId, IContainer container);

        void RemoveItem(IItem item);
    }
}
