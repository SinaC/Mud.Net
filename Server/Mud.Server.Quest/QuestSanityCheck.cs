using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Quest
{
    [Export(typeof(ISanityCheck)), Shared]
    public class QuestSanityCheck : ISanityCheck
    {
        private ILogger<QuestSanityCheck> Logger { get; }
        private IQuestManager QuestManager { get; }
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private GeneratedQuestOptions GeneratedQuestOptions { get; }

        public QuestSanityCheck(ILogger<QuestSanityCheck> logger, IQuestManager questManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IOptions<GeneratedQuestOptions> generatedQuestOptions)
        {
            Logger = logger;
            QuestManager = questManager;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
            GeneratedQuestOptions = generatedQuestOptions.Value;
        }

        public bool PerformSanityChecks()
        {
            var fatalErrorFound = false;
            // check quest item blueprints
            foreach (var itemToFindBlueprintId in GeneratedQuestOptions.ItemToFindBlueprintIds)
            {
                var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(itemToFindBlueprintId);
                if (itemQuestBlueprint == null)
                    Logger.LogError("ItemQuest id {blueprintId} needed for generated quests doesn't exist.", itemToFindBlueprintId);
            }
            // check quest blueprints
            var duplicateQuestIds = QuestManager.QuestBlueprints
                .GroupBy(x => x.Id)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key)
                .ToArray();
            foreach(var duplicateQuestId in duplicateQuestIds)
                Logger.LogCritical("Quest id {blueprintId} has been found on multiple quest.", duplicateQuestId);
            if (duplicateQuestIds.Length > 0)
                fatalErrorFound = true;
            // check quest objectives
            foreach (var questBlueprint in QuestManager.QuestBlueprints)
            {
                if (questBlueprint.LootItemObjectives?.Length == 0 && questBlueprint.FloorItemObjectives?.Length == 0 && questBlueprint.KillObjectives?.Length == 0 && questBlueprint.LocationObjectives?.Length == 0)
                    Logger.LogError("Quest id {blueprintId} doesn't have any objectives.", questBlueprint.Id);
                else
                {
                    // check if some objectives have duplicate ids
                    var duplicateObjectiveIds = 
                        (questBlueprint.LootItemObjectives ?? Enumerable.Empty<QuestLootItemObjectiveBlueprint>()).Select(x => x.Id)
                        .Union((questBlueprint.FloorItemObjectives ?? Enumerable.Empty<QuestFloorItemObjectiveBlueprint>()).Select(x => x.Id))
                        .Union((questBlueprint.KillObjectives ?? Enumerable.Empty<QuestKillObjectiveBlueprint>()).Select(x => x.Id))
                        .Union((questBlueprint.LocationObjectives ?? Enumerable.Empty<QuestLocationObjectiveBlueprint>()).Select(x => x.Id))
                        .GroupBy(x => x, (id, ids) => new { objectiveId = id, count = ids.Count() }).Where(x => x.count > 1).ToArray();
                    foreach (var duplicateId in duplicateObjectiveIds)
                        Logger.LogCritical("Quest id {blueprintId} has objectives with duplicate id {objectiveId} count {count}.", questBlueprint.Id, duplicateId.objectiveId, duplicateId.count);
                    if (duplicateObjectiveIds.Length > 0)
                        fatalErrorFound = true;
                    // kill loot table
                    if (questBlueprint.KillLootTable != null && questBlueprint.KillLootTable.Count > 0)
                    {
                        foreach (var lootTable in questBlueprint.KillLootTable)
                        {
                            var targetBlueprint = CharacterManager.GetCharacterBlueprint(lootTable.Key);
                            if (targetBlueprint == null)
                                Logger.LogError("Quest id {blueprintId} character id {characterBlueprintId} found in kill loot table doesn't exist.", questBlueprint.Id, lootTable.Key);
                            foreach (var lootEntry in lootTable.Value.Entries)
                            {
                                var lootBlueprint = ItemManager.GetItemBlueprint(lootEntry.Value);
                                if (lootBlueprint == null)
                                    Logger.LogError("Quest id {blueprintId} item id {itemBlueprintId} found in kill table for character id {characterBlueprintId} doesn't exist.", questBlueprint.Id, lootEntry.Value, lootTable.Key);
                            }
                        }
                    }
                    // loot item objectives
                    if (questBlueprint.LootItemObjectives != null && questBlueprint.LootItemObjectives.Length > 0)
                    {
                        foreach (var lootItemObjective in questBlueprint.LootItemObjectives)
                        {
                            var itemBlueprint = ItemManager.GetItemBlueprint(lootItemObjective.ItemBlueprintId);
                            if (itemBlueprint == null)
                                Logger.LogError("Quest id {blueprintId} item id {itemBlueprintId} found in loot item objectives doesn't exist.", questBlueprint.Id, lootItemObjective.ItemBlueprintId);
                        }
                    }
                    // floor item objectives
                    if (questBlueprint.FloorItemObjectives != null && questBlueprint.FloorItemObjectives.Length > 0)
                    {
                        //QuestSanityCheck: also check if QuestFloorItemObjectiveBlueprint is coherent(SpawnCountOnRequest cannot be greater than MaxInWorld and not greater than MaxInRoom * RoomBlueprintIds.Length)
                        foreach (var floorItemObjective in questBlueprint.FloorItemObjectives)
                        {
                            if (floorItemObjective.SpawnCountOnRequest > floorItemObjective.MaxInWorld)
                                Logger.LogError("Quest id {blueprintId} floor item objective {objectiveId} has SpawnCountOnRequest {spawnCountOnRequest} > MaxInWorld {maxInWorld}.", questBlueprint.Id, floorItemObjective.Id, floorItemObjective.SpawnCountOnRequest, floorItemObjective.MaxInWorld);
                            if (floorItemObjective.SpawnCountOnRequest > floorItemObjective.MaxInRoom * floorItemObjective.RoomBlueprintIds.Length)
                                Logger.LogError("Quest id {blueprintId} floor item objective {objectiveId} has SpawnCountOnRequest {spawnCountOnRequest} > MaxInRoom {maxInRoom} * Spawn locations {spawnLocationCount}.", questBlueprint.Id, floorItemObjective.Id, floorItemObjective.SpawnCountOnRequest, floorItemObjective.MaxInRoom, floorItemObjective.RoomBlueprintIds.Length);
                            var itemBlueprint = ItemManager.GetItemBlueprint(floorItemObjective.ItemBlueprintId);
                            if (itemBlueprint == null)
                                Logger.LogError("Quest id {blueprintId} item id {itemBlueprintId} found in floor item objectives doesn't exist.", questBlueprint.Id, floorItemObjective.ItemBlueprintId);
                            if (floorItemObjective.RoomBlueprintIds.Length == 0)
                                Logger.LogError("Quest id {blueprintId} floor item objective {objectiveId} has no spawn room location.", questBlueprint.Id, floorItemObjective.Id);
                            foreach (var roomBlueprintId in floorItemObjective.RoomBlueprintIds)
                            {
                                var roomBlueprint = RoomManager.GetRoomBlueprint(roomBlueprintId);
                                if (roomBlueprint == null)
                                    Logger.LogError("Quest id {blueprintId} room id {roomBlueprintId} found in floor item objectives doesn't exist.", questBlueprint.Id, roomBlueprintId);
                            }
                        }
                    }
                    // kill objectives
                    if (questBlueprint.KillObjectives != null && questBlueprint.KillObjectives.Length > 0)
                    {
                        foreach (var killObjective in questBlueprint.KillObjectives)
                        {
                            var targetBlueprint = CharacterManager.GetCharacterBlueprint(killObjective.CharacterBlueprintId);
                            if (targetBlueprint == null)
                                Logger.LogError("Quest id {blueprintId} character id {characterBlueprintId} found in kill objectives doesn't exist.", questBlueprint.Id, killObjective.CharacterBlueprintId);
                        }
                    }
                    // location objectives
                    if (questBlueprint.LocationObjectives != null && questBlueprint.LocationObjectives.Length > 0)
                    {
                        foreach (var locationObjective in questBlueprint.LocationObjectives)
                        {
                            var roomBlueprint = RoomManager.GetRoomBlueprint(locationObjective.RoomBlueprintId);
                            if (roomBlueprint == null)
                                Logger.LogError("Quest id {blueprintId} room id {roomBlueprintId} found in location objectives doesn't exist.", questBlueprint.Id, locationObjective.RoomBlueprintId);
                        }
                    }
                }
            }
            return fatalErrorFound;
        }
    }
}
