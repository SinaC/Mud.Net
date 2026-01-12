using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Blueprints.Item;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest.Objectives;
using Mud.Server.Random;

namespace Mud.Server.Quest;

[Export(typeof(IGeneratedQuest))]
public class GeneratedQuest : QuestBase, IGeneratedQuest
{
    private readonly Dictionary<int, QuestKillLootTable<int>> _killLootTable;

    private IServiceProvider ServiceProvider { get; }
    private IRandomManager RandomManager { get; }

    public GeneratedQuest(ILogger<GeneratedQuest> logger, IServiceProvider serviceProvider, IItemManager itemManager, ITimeManager timeManager, IRandomManager randomManager)
        : base(logger, itemManager, timeManager)
    {
        _killLootTable = [];

        ServiceProvider = serviceProvider;
        RandomManager = randomManager;
    }

    protected override bool ShouldQuestItemBeDestroyed => true;

    protected void Initialize(IPlayableCharacter character, INonPlayableCharacter giver, IRoom room, int level, int timeLimit)
    {
        Room = room;
        Character = character;
        Giver = giver;
        StartTime = TimeManager.CurrentTime;
        PulseLeft = Pulse.FromMinutes(timeLimit);
        Giver = giver;
        Level = level;
        TimeLimit = timeLimit;

        character.IncrementStatistics(AvatarStatisticTypes.GeneratedQuestsRequested);
    }


    public bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, int itemQuestBlueprintId, IRoom room, int level, int timeLimit)
    {
        Initialize(character, giver, room, level, timeLimit);

        var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(itemQuestBlueprintId);
        if (itemQuestBlueprint == null)
        {
            Logger.LogError("GeneratedQuest: quest item {blueprintId} doesn't exist", itemQuestBlueprintId);
            return false;
        }
        var itemQuest = ItemManager.AddItem<IItemQuest>(Guid.NewGuid(), itemQuestBlueprintId, room);
        if (itemQuest == null)
        {
            Logger.LogError("GeneratedQuest: cannot create quest item {blueprintId}", itemQuestBlueprintId);
            return false;
        }
        itemQuest.AddBaseItemFlags(false, "StayDeath");

        GeneratedQuestType = GeneratedQuestType.FindItem;
        ItemQuestBlueprint = itemQuestBlueprint;
        Title = $"Recover the fabled {itemQuestBlueprint.ShortDescription} in {room.Area.DisplayName}";
        // TODO: description
        Description = null;

        var itemQuestObjective = new FloorItemQuestObjective { ItemBlueprint = itemQuestBlueprint, Total = 1 };
        _objectives.Add(itemQuestObjective);
        itemQuest.SetTimer(TimeSpan.FromMinutes(timeLimit + 5)); // make sure to destroy item (just in case we missed the destroy)
        itemQuest.AddBaseItemFlags(false, "NoDrop"); // to be sure quest item cannot be dropped/given/sold/...
        return true;
    }

    public bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, int itemQuestBlueprintId, INonPlayableCharacter target, IRoom room, int timeLimit)
    {
        Initialize(character, giver, room, target.Level, timeLimit);

        var itemQuestBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(itemQuestBlueprintId);
        if (itemQuestBlueprint == null)
        {
            Logger.LogError("GeneratedQuest: quest item {blueprintId} doesn't exist", itemQuestBlueprintId);
            return false;
        }

        GeneratedQuestType = GeneratedQuestType.LootItem;
        ItemQuestBlueprint = itemQuestBlueprint;
        Target = target;
        Title = $"Recover the fabled {itemQuestBlueprint.ShortDescription} from {target.Blueprint.ShortDescription} in {room.Area.DisplayName}";
        // TODO: description
        Description = null;

        var questKillLootTable = ServiceProvider.GetRequiredService<QuestKillLootTable<int>>();
        questKillLootTable.AddItem(itemQuestBlueprint.Id, 100); // TODO: 100% ?
        var itemQuestObjective = new LootItemQuestObjective { ItemBlueprint = itemQuestBlueprint, Total = 1 };
        var locationQuestObject = new LocationQuestObjective { RoomBlueprint = room.Blueprint };
        _objectives.Add(itemQuestObjective);
        _objectives.Add(locationQuestObject);
        _killLootTable.Add(target.Blueprint.Id, questKillLootTable);
        return true;
    }

    public bool Initialize(IPlayableCharacter character, INonPlayableCharacter giver, INonPlayableCharacter target, IRoom room, int timeLimit)
    {
        Initialize(character, giver, room, target.Level, timeLimit);

        GeneratedQuestType = GeneratedQuestType.KillMob;
        ItemQuestBlueprint = null;
        Target = target;
        Title = $"Slay the dreaded {target.Blueprint.ShortDescription} in {room.Area.DisplayName}";
        // TODO: description
        Description = null;

        var killQuestObjective = new KillQuestObjective { TargetBlueprint = target.Blueprint, Total = 1 };
        var locationQuestObject = new LocationQuestObjective { RoomBlueprint = room.Blueprint };
        _objectives.Add(killQuestObjective);
        _objectives.Add(locationQuestObject);
        return true;
    }

    public GeneratedQuestType GeneratedQuestType { get; private set; }

    public INonPlayableCharacter Target { get; private set; } = null!;

    public ItemQuestBlueprint? ItemQuestBlueprint { get; private set; }

    public IRoom Room { get; private set; } = null!;

    public void Delete()
    {
        DestroyQuestItems();
    }

    #region IQuest

    public override string DebugName => $"{Title}[Generated]";

    public override string Title { get; protected set; } = null!;

    public override string? Description { get; protected set; }

    public override int Level { get; protected set; }

    public override int TimeLimit { get; protected set; }

    public override IReadOnlyDictionary<int, QuestKillLootTable<int>> KillLootTable => _killLootTable;

    public override void Timeout()
    {
        base.Timeout();

        Character.IncrementStatistics(AvatarStatisticTypes.GeneratedQuestsTimedout);
    }

    public override void Complete()
    {
        Character.IncrementStatistics(AvatarStatisticTypes.GeneratedQuestsCompleted);

        if (_objectives.OfType<ItemQuestObjectiveBase>() != null)
            DestroyQuestItems();

        // rewards
        var moneyReward = 1500 + RandomManager.Next(13500);
        var goldReward = moneyReward / 100;
        var silverReward = moneyReward - (goldReward * 100);
        var pointReward = 5 + RandomManager.Next(20);
        var practiceReward = RandomManager.Chance(8) ? (1 + RandomManager.Next(7)) : 0;

        Character.Act(ActOptions.ToCharacter, "{0:N} tells you 'Congratulations on completing your quest!'", Giver);
        if (practiceReward > 0)
            Character.Act(ActOptions.ToCharacter, "{0:N} tells you 'As a reward, I am giving you {1} quest points, {2} practices, and {3} silver coins.'", Giver, pointReward, practiceReward, moneyReward);
        else
            Character.Act(ActOptions.ToCharacter, "{0:N} tells you 'As a reward, I am giving you {1} quest points, and {2} silver coins.'", Giver, pointReward, moneyReward);
        Character.UpdateMoney(silverReward, goldReward);
        Character.UpdateCurrency(Currencies.QuestPoints, pointReward);
        Character.UpdateTrainsAndPractices(0, practiceReward);

        // set timer to 20 minutes for next quest
        Character.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(20));

        CompletionTime = TimeManager.CurrentTime;
    }

    public override void Abandon()
    {
        base.Abandon();

        Character.IncrementStatistics(AvatarStatisticTypes.GeneratedQuestsAbandoned);

        Character.Act(ActOptions.ToCharacter, "{0:N} tells you 'You surprise me {1}.", Giver, Character.DisplayName);
        Character.Act(ActOptions.ToCharacter, "{0:N} tells you 'I thought you could handle a such simple task.", Giver);

        // set timer to 15 minutes for next quest
        Character.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(15));
    }

    protected override void DestroyQuestItems()
    {
        base.DestroyQuestItems();
        if (GeneratedQuestType == GeneratedQuestType.FindItem)
        {
            var item = Room.Content.FirstOrDefault(x => x.Blueprint.Id == ItemQuestBlueprint!.Id);
            if (item != null)
                ItemManager.RemoveItem(item);
        }
    }

    #endregion
}
