using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Quest.Objectives;
using Mud.Random;
using System.Security.Authentication;

namespace Mud.Server.Quest;

[Export(typeof(IPredefinedQuest))]
public class PredefinedQuest : QuestBase, IPredefinedQuest
{
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IRandomManager RandomManager { get; }
    private int MaxLevel { get; }

    public PredefinedQuest(ILogger<PredefinedQuest> logger, IOptions<WorldOptions> worldOptions, ITimeManager timeManager, IItemManager itemManager, IRoomManager roomManager, ICharacterManager characterManager, IRandomManager randomManager)
        : base(logger, itemManager, timeManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        RandomManager = randomManager;
        MaxLevel = worldOptions.Value.MaxLevel;
    }

    protected override bool ShouldQuestItemBeDestroyed => Blueprint.ShouldQuestItemBeDestroyed;

    public QuestBlueprint Blueprint { get; private set; } = null!;

    public void Initialize(QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver) // TODO: giver should be ICharacterQuestor
    {
        Character = character;
        StartTime = TimeManager.CurrentTime;
        PulseLeft = Pulse.FromMinutes(blueprint.TimeLimit);
        Blueprint = blueprint;
        Giver = giver;
        BuildObjectives(character);

        Character.IncrementStatistics(AvatarStatisticTypes.PredefinedQuestsRequested);
    }

    public bool Initialize(QuestBlueprint blueprint, ActiveQuestData questData, IPlayableCharacter character)
    {
        Character = character;
        // don't update statistics, we were already on that quest

        // Extract informations from ActiveQuestData
        Blueprint = blueprint;
        StartTime = questData.StartTime;
        PulseLeft = questData.PulseLeft;
        CompletionTime = questData.CompletionTime;

        var characterQuestorBlueprint = CharacterManager.GetCharacterBlueprint<CharacterQuestorBlueprint>(questData.GiverId);
        if (characterQuestorBlueprint == null)
        {
            Logger.LogError("Quest giver blueprint id {giverId} not found!!!", questData.GiverId);
            return false;
        }
        else
        {
            var giver = CharacterManager.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint?.Id == characterQuestorBlueprint.Id && x.Room?.Blueprint?.Id == questData.GiverRoomId) ?? CharacterManager.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint?.Id == characterQuestorBlueprint.Id);
            if (giver == null)
            {
                Logger.LogError("Quest giver blueprint id {blueprintId} room blueprint Id {giverRoomId} not found!!!", questData.GiverId, questData.GiverRoomId);
                return false;
            }
            else
                Giver = giver;
        }

        BuildObjectives(character);
        foreach (var objectiveData in questData.Objectives)
        {
            // Search objective
            var objective = Objectives.FirstOrDefault(x => x.Id == objectiveData.ObjectiveId);
            switch (objective)
            {
                case QuestObjectiveCountBase questObjectiveCountBase:
                    questObjectiveCountBase.Count = objectiveData.Count;
                    break;
                // TODO: if quest item: test if conflict between stored count and inventory
                case LocationQuestObjective questObjectiveLocation:
                    questObjectiveLocation.Explored = objectiveData.Count > 0;
                    break;
                default:
                    Logger.LogError("Quest ({questId}) objective ({objectiveId}) cannot be found for character {name}.", questData.QuestId, objectiveData.ObjectiveId, character.DisplayName);
                    break;
            }
        }
        return true;
    }

    public void SpawnQuestItemOnFloorIfNeeded()
    {
        if (Blueprint.FloorItemObjectives == null)
            return;
        foreach (var questFloorItemObjectiveBlueprint in Blueprint.FloorItemObjectives)
        {
            var roomBlueprintId = RandomManager.Random(questFloorItemObjectiveBlueprint.RoomBlueprintIds);
            var locationBlueprint = RoomManager.GetRoomBlueprint(roomBlueprintId);
            if (locationBlueprint == null)
            {
                Logger.LogWarning("Floor item objective room {roomBlueprintId} doesn't exist for quest {blueprintId}", roomBlueprintId, Blueprint.Id);
                return;
            }

            var room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == locationBlueprint.Id);
            if (room == null)
            {
                Logger.LogWarning("Floor item objective room {roomBlueprintId} doesn't exist for quest {blueprintId}", roomBlueprintId, Blueprint.Id);
                return;
            }
            var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(questFloorItemObjectiveBlueprint.ItemBlueprintId);
            if (itemQuestBlueprint == null)
            {
                Logger.LogWarning("Floor item objective item {itemBlueprintId} doesn't exist (or is not quest item) for quest {blueprintId}", questFloorItemObjectiveBlueprint.ItemBlueprintId, Blueprint.Id);
                return;
            }

            SpawnQuestItemForObjectiveOnFloorIfNeeded(questFloorItemObjectiveBlueprint, itemQuestBlueprint, room);
        }
    }

    public ICompletedQuest? GenerateCompletedQuest()
    {
        if (!AreObjectivesFulfilled || CompletionTime is null)
            return null;
        return new CompletedQuest
        {
            QuestId = Blueprint.Id,
            QuestBlueprint = Blueprint,
            StartTime = StartTime,
            CompletionTime = CompletionTime!.Value
        };
    }

    public ActiveQuestData MapQuestData()
    {
        return new()
        {
            QuestId = Blueprint.Id,
            StartTime = StartTime,
            PulseLeft = PulseLeft,
            CompletionTime = CompletionTime,
            GiverId = Giver.Blueprint.Id,
            GiverRoomId = Giver.Room?.Blueprint.Id ?? 0,
            Objectives = Objectives.Select(x => new ActiveQuestObjectiveData
            {
                ObjectiveId = x.Id,
                Count = ComputeObjectiveActiveQuestObjectiveDataCount(x)
            }).ToArray()
        };
    }

    #region IQuest

    public override string DebugName => $"{Title}[{Blueprint.Id}]";

    public override string Title
    {
        get => Blueprint.Title;
        protected set { /*nop*/ }
    }

    public override string? Description
    {
        get => Blueprint.Description;
        protected set { /*nop*/ }
    }

    public override int Level
    {
        get => Blueprint.Level;
        protected set { /*nop*/ }
    }

    public override int TimeLimit
    {
        get => Blueprint.TimeLimit;
        protected set { /*nop*/ }
    }

    public override IReadOnlyDictionary<int, QuestKillLootTable<int>> KillLootTable => Blueprint.KillLootTable;

    public override void Timeout()
    {
        base.Timeout();

        Character.IncrementStatistics(AvatarStatisticTypes.PredefinedQuestsTimedout);
    }

    public override void Complete()
    {
        Character.IncrementStatistics(AvatarStatisticTypes.PredefinedQuestsCompleted);

        // TODO: give xp/gold/loot
        if (Blueprint.ShouldQuestItemBeDestroyed && _objectives.OfType<ItemQuestObjectiveBase>() != null)
            DestroyQuestItems();

        int xpGain = 0;
        int goldGain = Blueprint.Gold;

        // XP: http://wow.gamepedia.com/Experience_point#Quest_XP
        if (Character.Level < MaxLevel)
        {
            int factorPercentage = 100;
            if (Character.Level == Blueprint.Level + 6)
                factorPercentage = 80;
            else if (Character.Level == Blueprint.Level + 7)
                factorPercentage = 60;
            else if (Character.Level == Blueprint.Level + 8)
                factorPercentage = 40;
            else if (Character.Level == Blueprint.Level + 9)
                factorPercentage = 20;
            else if (Character.Level >= Blueprint.Level + 10)
                factorPercentage = 10;
            xpGain = (Blueprint.Experience * factorPercentage) / 100;
        }
        else
            goldGain = Blueprint.Experience * 6;

        // Display
        Character.Send("%y%You receive {0} experience and {1} gold.%x%", xpGain, goldGain);

        // Give rewards
        if (xpGain > 0)
            Character.GainExperience(xpGain);
        if (goldGain > 0)
            Character.UpdateMoney(0, goldGain);

        CompletionTime = TimeManager.CurrentTime;
    }

    public override void Abandon()
    {
        base.Abandon();

        Character.IncrementStatistics(AvatarStatisticTypes.PredefinedQuestsAbandoned);
    }

    #endregion

    private int ComputeObjectiveActiveQuestObjectiveDataCount(IQuestObjective questObjective)
    {
        switch (questObjective)
        {
            case QuestObjectiveCountBase questObjectiveCountBase:
                return questObjectiveCountBase.Count;
            case LocationQuestObjective questObjectiveLocation:
                return questObjectiveLocation.Explored ? 1 : 0;
            default:
                Logger.LogError("Cannot convert quest objective {objectiveId} type {type} to count", questObjective.Id, questObjective.GetType().Name);
                break;
        }

        return 0;
    }

    private void BuildObjectives(ICharacter character)
    {
        BuildLootItemObjectives(character);
        BuildFloorItemObjectives(character);
        BuildKillObjectives(character);
        BuildLocationObjectives(character);
    }

    private void BuildLootItemObjectives(ICharacter character)
    {
        if (Blueprint.LootItemObjectives == null)
            return;
        foreach (var lootItemObjectiveBlueprint in Blueprint.LootItemObjectives)
        {
            var itemBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(lootItemObjectiveBlueprint.ItemBlueprintId);
            if (itemBlueprint != null)
                _objectives.Add(new LootItemQuestObjective
                {
                    Id = lootItemObjectiveBlueprint.Id,
                    ItemBlueprint = itemBlueprint,
                    Count = character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == lootItemObjectiveBlueprint.ItemBlueprintId), // should always be 0
                    Total = lootItemObjectiveBlueprint.Count
                });
            else
                Logger.LogWarning("Loot item objective {itemBlueprintId} doesn't exist (or is not quest item) for quest {blueprintId}", lootItemObjectiveBlueprint.ItemBlueprintId, Blueprint.Id);
        }
    }

    private void BuildFloorItemObjectives(ICharacter character)
    {
        if (Blueprint.FloorItemObjectives == null)
            return;
        foreach (var questFloorItemObjectiveBlueprint in Blueprint.FloorItemObjectives)
        {
            if (questFloorItemObjectiveBlueprint.RoomBlueprintIds == null || questFloorItemObjectiveBlueprint.RoomBlueprintIds.Length == 0)
                Logger.LogError("Floor item object doesn't contain any room for quest {blueprintId}", Blueprint.Id);
            else
            {
                var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(questFloorItemObjectiveBlueprint.ItemBlueprintId);
                if (itemQuestBlueprint == null)
                    Logger.LogWarning("Floor item objective item {itemBlueprintId} doesn't exist (or is not quest item) for quest {blueprintId}", questFloorItemObjectiveBlueprint.ItemBlueprintId, Blueprint.Id);
                else
                {
                    // count item instance in character
                    var alreadyFoundItemCountOnCharacter = character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == questFloorItemObjectiveBlueprint.ItemBlueprintId);
                    _objectives.Add(new FloorItemQuestObjective
                    {
                        Id = questFloorItemObjectiveBlueprint.Id,
                        ItemBlueprint = itemQuestBlueprint,
                        Count = alreadyFoundItemCountOnCharacter,
                        Total = questFloorItemObjectiveBlueprint.Count,
                        RoomBlueprintIds = questFloorItemObjectiveBlueprint.RoomBlueprintIds,
                    });

                    // spawn quest item(s) if needed
                    // if already enough on character item to complete objective, don't spawn
                    if (alreadyFoundItemCountOnCharacter < questFloorItemObjectiveBlueprint.Count)
                    {
                        var missingItemCount = questFloorItemObjectiveBlueprint.Count - alreadyFoundItemCountOnCharacter;
                        var spawnCount = Math.Min(missingItemCount, questFloorItemObjectiveBlueprint.SpawnCountOnRequest); // no more then SpawnCountOnRequest nor missing count
                        for (var i = 0; i < spawnCount; i++)
                        {
                            var roomBlueprintId = RandomManager.Random(questFloorItemObjectiveBlueprint.RoomBlueprintIds);
                            var locationBlueprint = RoomManager.GetRoomBlueprint(roomBlueprintId);
                            if (locationBlueprint == null)
                                Logger.LogWarning("Floor item objective room {roomBlueprintId} doesn't exist for quest {blueprintId}", roomBlueprintId, Blueprint.Id);
                            else
                            {
                                var room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == locationBlueprint.Id);
                                if (room == null)
                                    Logger.LogWarning("Floor item objective room {roomBlueprintId} doesn't exist for quest {blueprintId}", roomBlueprintId, Blueprint.Id);
                                else
                                {
                                    // spawn quest item on the floor if needed
                                    SpawnQuestItemForObjectiveOnFloorIfNeeded(questFloorItemObjectiveBlueprint, itemQuestBlueprint, room);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void BuildKillObjectives(ICharacter character)
    {
        if (Blueprint.KillObjectives == null)
            return;
        foreach (var killObjectiveBlueprint in Blueprint.KillObjectives)
        {
            var characterBlueprint = CharacterManager.GetCharacterBlueprint(killObjectiveBlueprint.CharacterBlueprintId);
            if (characterBlueprint != null)
                _objectives.Add(new KillQuestObjective
                {
                    Id = killObjectiveBlueprint.Id,
                    TargetBlueprint = characterBlueprint,
                    Count = 0,
                    Total = killObjectiveBlueprint.Count
                });
            else
                Logger.LogWarning("Kill objective {objectiveBlueprintId} doesn't exist for quest {blueprintId}", killObjectiveBlueprint.CharacterBlueprintId, Blueprint.Id);
        }
    }

    private void BuildLocationObjectives(ICharacter character)
    {
        if (Blueprint.LocationObjectives == null)
            return;
        foreach (var locationObjectiveBlueprint in Blueprint.LocationObjectives)
        {
            var roomBlueprint = RoomManager.GetRoomBlueprint(locationObjectiveBlueprint.RoomBlueprintId);
            if (roomBlueprint != null)
                _objectives.Add(new LocationQuestObjective
                {
                    Id = locationObjectiveBlueprint.Id,
                    RoomBlueprint = roomBlueprint,
                    Explored = character.Room?.Blueprint?.Id == roomBlueprint.Id
                });
            else
                Logger.LogWarning("Location objective {objectiveBlueprintId} doesn't exist for quest {blueprintId}", locationObjectiveBlueprint.RoomBlueprintId, Blueprint.Id);
        }
    }

    private void SpawnQuestItemForObjectiveOnFloorIfNeeded(QuestFloorItemObjectiveBlueprint questFloorItemObjectiveBlueprint, ItemQuestBlueprint itemQuestBlueprint, IRoom room)
    {
        // count item instance on the room (if already one item on the floor, don't spawn)
        var roomInstanceCount = room.Content.OfType<IItemQuest>().Count(x => x.Blueprint.Id == questFloorItemObjectiveBlueprint.ItemBlueprintId);
        if (roomInstanceCount >= questFloorItemObjectiveBlueprint.MaxInRoom)
            return;
        // if count item instances in world (if it exceeds max instances allowed in quest, don't spawn)
        var worldInstanceCount = RoomManager.Rooms.SelectMany(x => x.Content).OfType<IItemQuest>().Count(x => x.Blueprint.Id == questFloorItemObjectiveBlueprint.ItemBlueprintId);
        if (worldInstanceCount >= questFloorItemObjectiveBlueprint.MaxInWorld)
            return;

        // create item on the floor
        var itemQuest = ItemManager.AddItem(Guid.NewGuid(), itemQuestBlueprint, $"PredefinedQuest[{Blueprint.Id}]", room);
        if (itemQuest == null)
        {
            Logger.LogError("Cannot create quest item {itemBlueprintId} for quest {blueprintId}", itemQuestBlueprint.Id, Blueprint.Id);
            return;
        }
        itemQuest.AddBaseItemFlags(false, "NoDrop"); // to be sure quest item cannot be dropped/given/sold/...
        if (TimeLimit > 0)
        {
            itemQuest.SetTimer(TimeSpan.FromMinutes(TimeLimit + 5)); // make sure to destroy item (just in case we missed the destroy)
        }
        Logger.LogInformation("Spawn quest item {itemBlueprintId} in room {roomId} for quest {blueprintId}", itemQuestBlueprint.Id, room.Blueprint.Id, Blueprint.Id);
    }
}
