﻿using System;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Quest;

namespace Mud.Server.Item
{
    public class ItemQuest : ItemBase<ItemQuestBlueprint, ItemData>, IItemQuest
    {
        public ItemQuest(Guid guid, ItemQuestBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            UpdateQuestObjective(containedInto, false);
        }

        public ItemQuest(Guid guid, ItemQuestBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            // don't call UpdateQuestObjective because it will increase objective item count each time the player reconnect
            // could maybe call it with force = true
        }

        #region ItemBase

        public override bool IsQuestObjective(IPlayableCharacter questingCharacter)
        {
            return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<ItemQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id);
        }

        public override bool ChangeContainer(IContainer container)
        {
            // TODO: cannot be get if not on that quest
            bool baseResult = base.ChangeContainer(container);
            if (baseResult)
                UpdateQuestObjective(container, false);
            return baseResult;
        }

        #endregion

        private void UpdateQuestObjective(IContainer container, bool force)
        {
            if (container is IPlayableCharacter character)
            {
                foreach (IQuest quest in character.Quests)
                    quest.Update(this, force);
            }
        }

    }
}
