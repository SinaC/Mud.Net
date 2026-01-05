using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Blueprints.Quest;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Quest;
using Microsoft.Extensions.Options;
using Mud.Server.Options;
using Mud.Server.Interfaces.Item;
using Mud.Blueprints.Item;

namespace Mud.Server.Quest
{
    [Export(typeof(ISanityCheck)), Shared]
    public class QuestSanityCheck : ISanityCheck
    {
        private ILogger<QuestSanityCheck> Logger { get; }
        private IQuestManager QuestManager { get; }
        private IItemManager ItemManager { get; }
        private QuestOptions QuestOptions { get; }

        public QuestSanityCheck(ILogger<QuestSanityCheck> logger, IQuestManager questManager, IItemManager itemManager, IOptions<QuestOptions> questOptions)
        {
            Logger = logger;
            QuestManager = questManager;
            ItemManager = itemManager;
            QuestOptions = questOptions.Value;
        }

        public bool PerformSanityChecks()
        {
            // check quest item blueprints
            foreach (var itemToFindBlueprintId in QuestOptions.ItemToFindBlueprintIds)
            {
                var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(itemToFindBlueprintId);
                if (itemQuestBlueprint == null)
                    Logger.LogError("ItemQuest id {blueprintId} doesn't exist", itemToFindBlueprintId);
            }
            // check quest blueprints
            foreach (var questBlueprint in QuestManager.QuestBlueprints)
            {
                if (questBlueprint.ItemObjectives?.Length == 0 && questBlueprint.KillObjectives?.Length == 0 && questBlueprint.LocationObjectives?.Length == 0)
                    Logger.LogError("Quest id {blueprintId} doesn't have any objectives.", questBlueprint.Id);
                else
                {
                    var duplicateIds = (questBlueprint.ItemObjectives ?? Enumerable.Empty<QuestItemObjectiveBlueprint>()).Select(x => x.Id).Union((questBlueprint.KillObjectives ?? Enumerable.Empty<QuestKillObjectiveBlueprint>()).Select(x => x.Id)).Union((questBlueprint.LocationObjectives ?? Enumerable.Empty<QuestLocationObjectiveBlueprint>()).Select(x => x.Id))
                        .GroupBy(x => x, (id, ids) => new { objectiveId = id, count = ids.Count() }).Where(x => x.count > 1);
                    foreach (var duplicateId in duplicateIds)
                        Logger.LogError("Quest id {blueprintId} has objectives with duplicate id {objectiveId} count {count}", questBlueprint.Id, duplicateId.objectiveId, duplicateId.count);
                }
            }
            return false;
        }
    }
}
