using Mud.POC.Abilities2.Domain;
using Mud.Server.Blueprints.Item;
using System;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IItemManager
    {
        IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
        IItem AddItem(Guid guid, ItemData itemData, IContainer container);
        IItem AddItem(Guid guid, int blueprintId, IContainer container);
    }
}
