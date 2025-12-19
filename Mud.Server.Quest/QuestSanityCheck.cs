using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Blueprints.Quest;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest
{
    [Export(typeof(ISanityCheck)), Shared]
    public class QuestSanityCheck : ISanityCheck
    {
        private ILogger<QuestSanityCheck> Logger { get; }
        private IQuestManager QuestManager { get; }

        public QuestSanityCheck(ILogger<QuestSanityCheck> logger, IQuestManager questManager)
        {
            Logger = logger;
            QuestManager = questManager;
        }

        public bool PerformSanityChecks()
        {
            foreach (QuestBlueprint questBlueprint in QuestManager.QuestBlueprints)
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
