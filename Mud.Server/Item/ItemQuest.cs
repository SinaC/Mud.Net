using System;
using System.Linq;
using Mud.Server.Blueprints.Item;
using Mud.Server.Quest;

namespace Mud.Server.Item
{
    public class ItemQuest : ItemBase<ItemQuestBlueprint>, IItemQuest
    {
        public ItemQuest(Guid guid, ItemQuestBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            UpdateQuestObjective(containedInto);
            IsWearable = false;
        }

        #region ItemBase

        public override bool IsQuestObjective(ICharacter questingCharacter)
        {
            return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<ItemQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id);
        }

        public override bool ChangeContainer(IContainer container)
        {
            // TODO: cannot be get if not on that quest
            bool baseResult = base.ChangeContainer(container);
            if (baseResult)
                UpdateQuestObjective(container);
            return baseResult;
        }

        private void UpdateQuestObjective(IContainer container)
        {
            if (container is ICharacter character && character.Impersonable)
            {
                foreach (IQuest quest in character.Quests)
                    quest.Update(this);
            }
        }

        #endregion
    }
}
