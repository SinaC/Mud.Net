using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Quest.Objectives;

namespace Mud.Server.Quest;

[Export(typeof(IPredefinedQuest))]
public class PredefinedQuest : QuestBase, IPredefinedQuest
{
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private int MaxLevel { get; }

    public PredefinedQuest(ILogger<PredefinedQuest> logger, IOptions<WorldOptions> worldOptions, ITimeManager timeManager, IItemManager itemManager, IRoomManager roomManager, ICharacterManager characterManager)
        : base(logger, itemManager, timeManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        MaxLevel = worldOptions.Value.MaxLevel;
    }

    protected override bool ShouldQuestItemBeDestroyed => Blueprint.ShouldQuestItemBeDestroyed;

    #region IPredefineQuest

    public QuestBlueprint Blueprint { get; private set; } = null!;

    public void Initialize(QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver) // TODO: giver should be ICharacterQuestor
    {
        Character = character;

        StartTime = TimeManager.CurrentTime;
        PulseLeft = blueprint.TimeLimit * Pulse.PulsePerMinutes;
        Blueprint = blueprint;
        Giver = giver;
        BuildObjectives(blueprint, character);
    }

    public bool Initialize(QuestBlueprint blueprint, CurrentQuestData questData, IPlayableCharacter character)
    {
        Character = character;

        // Extract informations from QuestData
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

        BuildObjectives(blueprint, character);
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

    #region IQuest

    public override string Title => Blueprint.Title;

    public override string Description =>  Blueprint.Description;

    public override int Level => Blueprint.Level;

    public override int TimeLimit => Blueprint.TimeLimit;

    public override IReadOnlyDictionary<int, QuestKillLootTable<int>> KillLootTable => Blueprint.KillLootTable;

    public override void Complete()
    {
        // TODO: give xp/gold/loot
        if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
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
            xpGain = (Blueprint.Experience*factorPercentage)/100;
        }
        else
            goldGain = Blueprint.Experience*6;

        // Display
        Character.Send("%y%You receive {0} experience and {1} gold.%x%", xpGain, goldGain);

        // Give rewards
        if (xpGain > 0)
            Character.GainExperience(xpGain);
        if (goldGain > 0)
            Character.UpdateMoney(0, goldGain);

        CompletionTime = TimeManager.CurrentTime;
    }

    #endregion

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
}
