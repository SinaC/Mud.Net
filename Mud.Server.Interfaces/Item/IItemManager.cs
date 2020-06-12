﻿using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Item
{
    public interface IItemManager
    {
        IEnumerable<IItem> Items { get; }

        IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
        IItem AddItem(Guid guid, ItemData itemData, IContainer container);
        IItem AddItem(Guid guid, int blueprintId, IContainer container);

        IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim);
        IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim, ICharacter killer);
        IItemMoney AddItemMoney(Guid guid, long silverCoins, long goldCoins, IContainer container);

        void RemoveItem(IItem item);
    }
}
