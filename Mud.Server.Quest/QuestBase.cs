using Microsoft.Extensions.Logging;
using Mud.Blueprints.Item;
using Mud.Blueprints.Quest;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest.Objectives;

namespace Mud.Server.Quest;

public abstract class QuestBase : IQuest
{
    protected readonly List<IQuestObjective> _objectives = [];

    protected ILogger<QuestBase> Logger { get; }
    protected IItemManager ItemManager { get; }
    protected ITimeManager TimeManager { get; }

    protected QuestBase(ILogger<QuestBase> logger, IItemManager itemManager, ITimeManager timeManager)
    {
        Logger = logger;
        ItemManager = itemManager;
        TimeManager = timeManager;
    }

    protected abstract bool ShouldQuestItemBeDestroyed { get; }

    #region IQuest

    public IPlayableCharacter Character { get; protected set; } = null!;

    public INonPlayableCharacter Giver { get; protected set; } = null!;

    public abstract string DebugName { get; }

    public abstract string Title { get; protected set; }

    public abstract string? Description { get; protected set; }

    public abstract int Level { get; protected set; }

    public bool AreObjectivesFulfilled => Objectives == null || Objectives.All(x => x.IsCompleted);

    public abstract int TimeLimit { get; protected set; }

    public DateTime StartTime { get; protected set; }

    public int PulseLeft { get; protected set; }

    public DateTime? CompletionTime { get; protected set; }

    public IEnumerable<IQuestObjective> Objectives => _objectives;

    public abstract IReadOnlyDictionary<int, QuestKillLootTable<int>> KillLootTable { get; }

    public void GenerateKillLoot(INonPlayableCharacter victim, IContainer container)
    {
        if (victim.Blueprint == null)
            return;
        if (!KillLootTable.TryGetValue(victim.Blueprint.Id, out var table))
            return;
        // generate only items which are not quest objective or not completed quest objective
        var forbiddenIds = new HashSet<int>();
        foreach(var itemQuestObjective in _objectives.OfType<LootItemQuestObjective>().Where(x => x.IsCompleted))
            forbiddenIds.Add(itemQuestObjective.ItemBlueprint.Id);
        var killLoots = table.GenerateLoots(forbiddenIds);
        if (killLoots != null)
        {
            foreach (var lootBlueprintId in killLoots)
            {
                var questItemBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(lootBlueprintId);
                if (questItemBlueprint != null)
                {
                    var item = ItemManager.AddItem(Guid.NewGuid(), questItemBlueprint, container);
                    if (item != null)
                        item.AddBaseItemFlags(false, "StayDeath");
                    Logger.LogDebug("Loot objective {lootBlueprintId} generated for {name}", lootBlueprintId, Character.DisplayName);
                }
                else
                {
                    Logger.LogError("Loot objective {lootBlueprintId} doesn't exist (or is not quest item) for quest {title}", lootBlueprintId, Title);
                }
            }
        }
    }

    public void Update(INonPlayableCharacter victim)
    {
        if (victim.Blueprint == null)
            return;
        if (AreObjectivesFulfilled)
            return;
        foreach (var objective in _objectives.OfType<KillQuestObjective>().Where(x => !x.IsCompleted && x.TargetBlueprint.Id == victim.Blueprint.Id))
        {
            objective.Count++;
            Character.Send($"%y%Quest '{Title}': {objective.CompletionState}%x%");
            if (AreObjectivesFulfilled)
                Character.Send($"%R%Quest '{Title}': complete%x%");
        }
    }

    public void Update(IItemQuest item, bool force)
    {
        if (item.Blueprint == null)
            return;
        // if forced, reset completion state and recount item in inventory
        if (force)
        {
            foreach (var objective in _objectives.OfType<ItemQuestObjectiveBase>().Where(x => x.ItemBlueprint.Id == item.Blueprint.Id))
                objective.Count = Character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == item.Blueprint.Id);
            return;
        }
        //
        if (AreObjectivesFulfilled)
            return;
        foreach (var objective in _objectives.OfType<ItemQuestObjectiveBase>().Where(x => !x.IsCompleted && x.ItemBlueprint.Id == item.Blueprint.Id))
        {
            objective.Count++;
            Character.Send($"%y%Quest '{Title}': {objective.CompletionState}%x%");
            if (AreObjectivesFulfilled)
                Character.Send($"%R%Quest '{Title}': complete%x%");
        }
    }

    public void Update(IRoom room)
    {
        if (room.Blueprint == null)
            return;
        if (AreObjectivesFulfilled)
            return;
        foreach (var objective in _objectives.OfType<LocationQuestObjective>().Where(x => !x.IsCompleted && x.RoomBlueprint.Id == room.Blueprint.Id))
        {
            objective.Explored = true;
            Character.Send($"%y%Quest '{Title}': {objective.CompletionState}%x%");
            if (AreObjectivesFulfilled)
                Character.Send($"%R%Quest '{Title}': complete%x%");
        }
    }

    public void Reset()
    {
        foreach (var objective in _objectives)
        {
            objective.Reset();
            if (objective is ItemQuestObjectiveBase itemQuestObjective)
                itemQuestObjective.Count = Character.Inventory.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemQuestObjective.ItemBlueprint.Id);
        }
    }

    public virtual void Timeout()
    {
        Character.Send($"%R%You have run out of time for quest '{Title}'.%x%");
        if (ShouldQuestItemBeDestroyed && _objectives.OfType<ItemQuestObjectiveBase>().Any())
            DestroyQuestItems();
    }

    public bool DecreasePulseLeft(int pulseCount)
    {
        if (PulseLeft < 0)
            return false;
        PulseLeft = Math.Max(PulseLeft - pulseCount, 0);
        return PulseLeft == 0;
    }

    public abstract void Complete();

    public virtual void Abandon()
    {
        // TODO: xp loss ?
        if (ShouldQuestItemBeDestroyed && _objectives.OfType<ItemQuestObjectiveBase>().Any())
            DestroyQuestItems();
    }

    #endregion

    protected virtual void DestroyQuestItems()
    {
        // Gather quest items
        var questItems = Character.Inventory.Where(x => x.Blueprint != null && _objectives.OfType<ItemQuestObjectiveBase>().Any(i => i.ItemBlueprint.Id == x.Blueprint.Id)).ToList();
        foreach (var questItem in questItems)
        {
            Logger.LogDebug("Destroying quest item {itemName} in {characterName}", questItem.DebugName, Character.DebugName);
            ItemManager.RemoveItem(questItem);
        }
    }
}
