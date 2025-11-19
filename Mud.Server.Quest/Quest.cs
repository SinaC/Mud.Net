using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Settings.Interfaces;

namespace Mud.Server.Quest;

public class Quest : IQuest
{
    private readonly List<IQuestObjective> _objectives;

    private ILogger Logger { get; }
    private ISettings Settings { get; }
    private ITimeManager TimeManager { get; }
    private IItemManager ItemManager { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IQuestManager QuestManager { get; }

    public Quest(ILogger logger, ISettings settings, ITimeManager timeManager, IItemManager itemManager, IRoomManager roomManager, ICharacterManager characterManager, IQuestManager questManager,
        QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver) // TODO: giver should be ICharacterQuestor
    {
        Logger = logger;
        Settings = settings;
        TimeManager = timeManager;
        ItemManager = itemManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        QuestManager = questManager;

        Character = character;
        StartTime = TimeManager.CurrentTime;
        PulseLeft = blueprint.TimeLimit * Pulse.PulsePerMinutes;
        Blueprint = blueprint;
        Giver = giver;
        _objectives = new List<IQuestObjective>();
        BuildObjectives(blueprint, character);
    }

    public Quest(ILogger logger, ISettings settings, ITimeManager timeManager, IItemManager itemManager, IRoomManager roomManager, ICharacterManager characterManager, IQuestManager questManager,
        CurrentQuestData questData, IPlayableCharacter character)
    {
        Logger = logger;
        Settings = settings;
        TimeManager = timeManager;
        ItemManager = itemManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        QuestManager = questManager;
        
        Character = character;

        // Extract informations from QuestData
        var questBlueprint = QuestManager.GetQuestBlueprint(questData.QuestId);
        // TODO: quid if blueprint is null?
        Blueprint = questBlueprint;
        StartTime = questData.StartTime;
        PulseLeft = questData.PulseLeft;
        CompletionTime = questData.CompletionTime;

        var characterQuestorBlueprint = CharacterManager.GetCharacterBlueprint<CharacterQuestorBlueprint>(questData.GiverId);
        if (characterQuestorBlueprint == null)
        {
            Logger.LogError("Quest giver blueprint id {giverId} not found!!!", questData.GiverId);
        }
        else
        {
            var giver = CharacterManager.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint?.Id == characterQuestorBlueprint.Id && x.Room?.Blueprint?.Id == questData.GiverRoomId) ?? CharacterManager.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint?.Id == characterQuestorBlueprint.Id);
            if (giver == null)
            {
                Logger.LogError("Quest giver blueprint id {blueprintId} room blueprint Id {giverRoomId} not found!!!", questData.GiverId, questData.GiverRoomId);
            }
            Giver = giver;
        }
        // TODO: if Giver is null, player will not be able to complete quest

        _objectives = [];
        BuildObjectives(questBlueprint, character);
        foreach (CurrentQuestObjectiveData objectiveData in questData.Objectives)
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
    }

    #region IQuest

    public QuestBlueprint Blueprint { get; }

    public IPlayableCharacter Character { get; }

    public INonPlayableCharacter Giver { get; }

    public bool IsCompleted => Objectives == null || Objectives.All(x => x.IsCompleted);

    public DateTime StartTime { get; }

    public int PulseLeft { get; private set; }

    public DateTime? CompletionTime { get; private set; }

    public IEnumerable<IQuestObjective> Objectives => _objectives;

    public void GenerateKillLoot(INonPlayableCharacter victim, IContainer container)
    {
        if (victim.Blueprint == null)
            return;
        if (!Blueprint.KillLootTable.TryGetValue(victim.Blueprint.Id, out var table))
            return;
        var killLoots = table.GenerateLoots();
        if (killLoots != null)
        {
            foreach (int loot in killLoots)
            {
                if (ItemManager.GetItemBlueprint(loot) is ItemQuestBlueprint questItemBlueprint)
                {
                    ItemManager.AddItem(Guid.NewGuid(), questItemBlueprint, container);
                    Logger.LogDebug($"Loot objective {loot} generated for {Character.DisplayName}");
                }
                else
                {
                    Logger.LogError("Loot objective {0} doesn't exist (or is not quest item) for quest {1}", loot, Blueprint.Id);
                }
            }
        }
    }

    public void Update(INonPlayableCharacter victim)
    {
        if (victim.Blueprint == null)
            return;
        if (IsCompleted)
            return;
        foreach (var objective in _objectives.OfType<KillQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == victim.Blueprint.Id))
        {
            objective.Count++;
            Character.Send($"%y%Quest '{Blueprint.Title}': {objective.CompletionState}%x%");
            if (IsCompleted)
                Character.Send($"%R%Quest '{Blueprint.Title}': complete%x%");
        }
    }

    public void Update(IItemQuest item, bool force)
    {
        if (item.Blueprint == null)
            return;
        // if forced, reset completion state and recount item in inventory
        if (force)
        {
            foreach (var objective in _objectives.OfType<ItemQuestObjective>().Where(x => x.Blueprint.Id == item.Blueprint.Id))
                objective.Count = Character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == item.Blueprint.Id);
            return;
        }
        //
        if (IsCompleted)
            return;
        foreach (var objective in _objectives.OfType<ItemQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == item.Blueprint.Id))
        {
            objective.Count++;
            Character.Send($"%y%Quest '{Blueprint.Title}': {objective.CompletionState}%x%");
            if (IsCompleted)
                Character.Send($"%R%Quest '{Blueprint.Title}': complete%x%");
        }
    }

    public void Update(IRoom room)
    {
        if (room.Blueprint == null)
            return;
        if (IsCompleted)
            return;
        foreach (LocationQuestObjective objective in _objectives.OfType<LocationQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == room.Blueprint.Id))
        {
            objective.Explored = true;
            Character.Send($"%y%Quest '{Blueprint.Title}': {objective.CompletionState}%x%");
            if (IsCompleted)
                Character.Send($"%R%Quest '{Blueprint.Title}': complete%x%");
        }
    }

    public void Reset()
    {
        foreach (IQuestObjective objective in _objectives)
        {
            objective.Reset();
            if (objective is ItemQuestObjective itemQuestObjective)
                itemQuestObjective.Count = Character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemQuestObjective.Blueprint.Id);
        }
    }

    public void Timeout()
    {
        Character.Send($"%R%You have run out of time for quest '{Blueprint.Title}'.%x%");
        if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
            DestroyQuestItems();
    }

    public bool DecreasePulseLeft(int pulseCount)
    {
        if (PulseLeft < 0)
            return false;
        PulseLeft = Math.Max(PulseLeft - pulseCount, 0);
        return PulseLeft == 0;
    }


    public void Complete()
    {
        // TODO: give xp/gold/loot
        if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
            DestroyQuestItems();

        int xpGain = 0;
        int goldGain = Blueprint.Gold;

        // XP: http://wow.gamepedia.com/Experience_point#Quest_XP
        if (Character.Level < Settings.MaxLevel)
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
            xpGain = (Blueprint.Experience*factorPercentage)/100;
        }
        else
            goldGain = Blueprint.Experience*6;

        // Display
        Character.Send("%y%You receive {0} exp and {1} gold.%x%", xpGain, goldGain);

        // Give rewards
        if (xpGain > 0)
            Character.GainExperience(xpGain);
        if (goldGain > 0)
            Character.UpdateMoney(0, goldGain);

        CompletionTime = TimeManager.CurrentTime;
    }

    public void Abandon()
    {
        // TODO: xp loss ?
        if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
            DestroyQuestItems();
    }

    public CurrentQuestData MapQuestData()
    {
        return new()
        {
            QuestId = Blueprint.Id,
            StartTime = StartTime,
            PulseLeft = PulseLeft,
            CompletionTime = CompletionTime,
            GiverId = Giver.Blueprint.Id,
            GiverRoomId = Giver.Room?.Blueprint.Id ?? 0,
            Objectives = Objectives.Select(x => new CurrentQuestObjectiveData
            {
                ObjectiveId = x.Id,
                Count = ComputeObjectiveCurrentQuestObjectiveDataCount(x)
            }).ToArray()
        };
    }

    #endregion

    private int ComputeObjectiveCurrentQuestObjectiveDataCount(IQuestObjective questObjective)
    {
        switch (questObjective)
        {
            case QuestObjectiveCountBase questObjectiveCountBase:
                return questObjectiveCountBase.Count;
            case LocationQuestObjective questObjectiveLocation:
                return questObjectiveLocation.Explored ? 1: 0;
            default:
                Logger.LogError("Cannot convert quest objective {objectiveId} type {type} to count", questObjective.Id, questObjective.GetType().Name);
                break;
        }

        return 0;
    }

    private void BuildObjectives(QuestBlueprint blueprint, ICharacter character)
    {
        if (Blueprint.ItemObjectives != null)
        {
            foreach (QuestItemObjectiveBlueprint itemObjective in Blueprint.ItemObjectives)
            {
                var itemBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(itemObjective.ItemBlueprintId);
                if (itemBlueprint != null)
                    _objectives.Add(new ItemQuestObjective
                    {
                        Id = itemObjective.Id,
                        Blueprint = itemBlueprint,
                        Count = character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemObjective.ItemBlueprintId), // should always be 0
                        Total = itemObjective.Count
                    });
                else
                    Logger.LogWarning("Loot objective {itemBlueprintId} doesn't exist (or is not quest item) for quest {blueprintId}", itemObjective.ItemBlueprintId, blueprint.Id);
            }
        }
        if (Blueprint.KillObjectives != null)
        {
            foreach (QuestKillObjectiveBlueprint killObjective in Blueprint.KillObjectives)
            {
                var characterBlueprint = CharacterManager.GetCharacterBlueprint(killObjective.CharacterBlueprintId);
                if (characterBlueprint != null)
                    _objectives.Add(new KillQuestObjective
                    {
                        Id = killObjective.Id,
                        Blueprint = characterBlueprint,
                        Count = 0,
                        Total = killObjective.Count
                    });
                else
                    Logger.LogWarning("Kill objective {objectiveBlueprintId} doesn't exist for quest {blueprintId}", killObjective.CharacterBlueprintId, blueprint.Id);
            }
        }
        if (Blueprint.LocationObjectives != null)
        {
            foreach (QuestLocationObjectiveBlueprint locationObjective in Blueprint.LocationObjectives)
            {
                var roomBlueprint = RoomManager.GetRoomBlueprint(locationObjective.RoomBlueprintId);
                if (roomBlueprint != null)
                    _objectives.Add(new LocationQuestObjective
                    {
                        Id = locationObjective.Id,
                        Blueprint = roomBlueprint,
                        Explored = character.Room?.Blueprint?.Id == roomBlueprint.Id
                    });
                else
                    Logger.LogWarning("Location objective {objectiveBlueprintId} doesn't exist for quest {blueprintId}", locationObjective.RoomBlueprintId, blueprint.Id);
            }
        }
    }

    private void DestroyQuestItems()
    {
        // Gather quest items
        var questItems = Character.Inventory.Where(x => x.Blueprint != null && Blueprint.ItemObjectives.Any(i => i.ItemBlueprintId == x.Blueprint.Id)).ToList();
        foreach (var questItem in questItems)
        {
            Logger.LogDebug("Destroying quest item {itemName} in {characterName}", questItem.DebugName, Character.DebugName);
            ItemManager.RemoveItem(questItem);
        }
    }
}
