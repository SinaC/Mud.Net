using Microsoft.Extensions.Logging;
using Mud.Blueprints.Item;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest.Objectives;
using Mud.Random;

namespace Mud.Server.Server;

[Export(typeof(ILootManager)), Shared]
public class LootManager : ILootManager
{
    private ILogger<LootManager> Logger { get; }
    private IItemManager ItemManager { get; }
    private IRandomManager RandomManager { get; }

    public LootManager(ILogger<LootManager> logger, IItemManager itemManager, IRandomManager randomManager)
    {
        Logger = logger;
        ItemManager = itemManager;
        RandomManager = randomManager;
    }

    public void GenerateLoots(IItemCorpse? corpse, ICharacter victim, IEnumerable<IPlayableCharacter> playableCharactersImpactedByKill)
    {
        var pcVictim = victim as IPlayableCharacter;
        var npcVictim = victim as INonPlayableCharacter;
        var room = victim.Room;

        // Money
        if (pcVictim != null)
            HandleMoneyOnDeath(pcVictim, corpse, room);
        else if (npcVictim != null)
            HandleMoneyOnDeath(npcVictim, corpse, room);

        // Items
        HandleItemsOnDeath(victim, corpse, room);

        // Quests
        if (npcVictim is not null)
        {
            // Check killer quest table (only if killer is PC and victim is NPC) // TODO: only visible for people on quest???
            foreach (var playableCharacterImpactedByKill in playableCharactersImpactedByKill)
            {
                foreach (var quest in playableCharacterImpactedByKill.ActiveQuests)
                {
                    // Generate loot on corpse
                    GenerateLootsFromQuests(quest, npcVictim, corpse, room);
                }
            }
        }

        // Loot tables
        if (npcVictim is not null)
            GenerateLootsFromTables(npcVictim, corpse, room);
    }

    private void HandleMoneyOnDeath(INonPlayableCharacter victim, IItemCorpse? corpse, IRoom room)
    {
        if (victim.NoLootOnDeath)
            return;

        var silver = victim.SilverCoins;
        var gold = victim.GoldCoins;

        if (corpse == null)
            ItemManager.AddItemMoney(Guid.NewGuid(), silver, gold, room); // TODO: Act(ActOptions.ToRoom, "{0} falls to the floor.", item); ?
        else
            ItemManager.AddItemMoney(Guid.NewGuid(), silver, gold, corpse);
    }

    private void HandleMoneyOnDeath(IPlayableCharacter victim, IItemCorpse? corpse, IRoom room)
    {
        if (victim.NoLootOnDeath)
            return;

        var silver = victim.SilverCoins;
        var gold = victim.GoldCoins;
        if (silver > 1 || gold > 1) // player keep half their money and leave the rest in the body
        {
            silver /= 2;
            gold /= 2;
            victim.UpdateMoney(-silver, -gold);
        }

        if (corpse == null)
            ItemManager.AddItemMoney(Guid.NewGuid(), silver, gold, room); // TODO: Act(ActOptions.ToRoom, "{0} falls to the floor.", item); ?
        else
            ItemManager.AddItemMoney(Guid.NewGuid(), silver, gold, corpse);
    }

    private void HandleItemsOnDeath(ICharacter victim, IItemCorpse? corpse, IRoom room)
    {
        // Fill corpse with inventory
        var inventory = victim.Inventory.ToArray();
        foreach (var item in inventory)
        {
            var result = HandleItemOnDeath(victim, item, corpse, room);
            switch (result)
            {
                case HandleItemOnDeathResults.MoveToCorpse:
                    item.ChangeContainer(corpse);
                    break;
                case HandleItemOnDeathResults.MoveToRoom:
                    item.ChangeContainer(room);
                    break;
                case HandleItemOnDeathResults.Destroy:
                    ItemManager.RemoveItem(item);
                    break;
            }
        }

        // Fill corpse with equipment
        var equipment = victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!).ToArray();
        foreach (var item in equipment)
        {
            var result = HandleItemOnDeath(victim, item, corpse, room);
            switch (result)
            {
                case HandleItemOnDeathResults.MoveToCorpse:
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(corpse);
                    break;
                case HandleItemOnDeathResults.MoveToRoom:
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(room);
                    break;
                case HandleItemOnDeathResults.Destroy:
                    ItemManager.RemoveItem(item);
                    break;
            }
        }
    }

    private enum HandleItemOnDeathResults
    {
        Nop, // stays on character
        MoveToCorpse, // move item to corpse
        MoveToRoom, // move item to room
        Destroy, // destroy item
    }

    private HandleItemOnDeathResults HandleItemOnDeath(ICharacter victim, IItem item, IItemCorpse? corpse, IRoom room)
    {
        if (victim.NoLootOnDeath)
            return HandleItemOnDeathResults.Destroy;
        if (item.ItemFlags.IsSet("Inventory"))
            return HandleItemOnDeathResults.Destroy;
        if (item.ItemFlags.IsSet("StayDeath"))
            return HandleItemOnDeathResults.Nop;
        if (item is IItemPotion)
            item.SetTimer(TimeSpan.FromMinutes(RandomManager.Range(500, 1000)));
        else if (item is IItemScroll)
            item.SetTimer(TimeSpan.FromMinutes(RandomManager.Range(1000, 2500)));
        if (item.ItemFlags.IsSet("VisibleDeath"))
            item.RemoveBaseItemFlags(false, "VisibleDeath");
        var isFloating = item.WearLocation == WearLocations.Float;
        if (isFloating)
        {
            if (item.ItemFlags.IsSet("RotDeath"))
            {
                if (item is IItemContainer container && container.Content.Any())
                {
                    victim.Act(ActOptions.ToRoom, "{0} evaporates, scattering its contents.", item);
                    var content = container.Content.ToArray();
                    foreach (var contentItem in content)
                        contentItem.ChangeContainer(room);
                }
                else
                    victim.Act(ActOptions.ToRoom, "{0} evaporates.", item);
                return HandleItemOnDeathResults.Destroy;
            }
            victim.Act(ActOptions.ToRoom, "{0} falls to the floor.", item);
            item.Recompute();
            return HandleItemOnDeathResults.MoveToRoom;
        }
        if (item.ItemFlags.IsSet("RotDeath"))
        {
            var duration = RandomManager.Range(5, 10);
            item.SetTimer(TimeSpan.FromMinutes(duration));
            item.RemoveBaseItemFlags(false, "RotDeath");
        }
        item.Recompute();
        if (corpse == null)
        {
            victim.Act(ActOptions.ToRoom, "{0} falls to the floor.", item);
            return HandleItemOnDeathResults.MoveToRoom;
        }
        return HandleItemOnDeathResults.MoveToCorpse;
    }

    private void GenerateLootsFromTables(INonPlayableCharacter victim, IItemCorpse? corpse, IRoom room)
    {
        if (victim.NoLootOnDeath)
            return;
        if (victim.Blueprint == null)
            return;
        if (victim.Blueprint.LootTable == null)
            return;

        var loots = victim.Blueprint.LootTable.GenerateLoots();
        if (loots != null && loots.Count != 0)
        {
            foreach (var lootBlueprintId in loots)
            {
                if (corpse == null)
                    ItemManager.AddItem(Guid.NewGuid(), lootBlueprintId, room); // TODO: Act(ActOptions.ToRoom, "{0} falls to the floor.", item); ?
                else
                    ItemManager.AddItem(Guid.NewGuid(), lootBlueprintId, corpse);
            }
        }
    }

    private void GenerateLootsFromQuests(IQuest quest, INonPlayableCharacter victim, IItemCorpse? corpse, IRoom room)
    {
        if (victim.Blueprint == null)
            return;
        if (!quest.KillLootTable.TryGetValue(victim.Blueprint.Id, out var table))
            return;
        // generate only items which are not quest objective or not completed quest objective
        var forbiddenIds = new HashSet<int>();
        foreach (var itemQuestObjective in quest.Objectives.OfType<LootItemQuestObjective>().Where(x => x.IsCompleted))
            forbiddenIds.Add(itemQuestObjective.ItemBlueprint.Id);
        var loots = table.GenerateLoots(forbiddenIds);
        if (loots != null)
        {
            foreach (var lootBlueprintId in loots)
            {
                var questItemBlueprint = ItemManager.GetItemBlueprint<ItemQuestBlueprint>(lootBlueprintId);
                if (questItemBlueprint != null)
                {
                    var item = corpse == null
                        ? ItemManager.AddItem(Guid.NewGuid(), questItemBlueprint, room) // TODO: Act(ActOptions.ToRoom, "{0} falls to the floor.", item); ?
                        : ItemManager.AddItem(Guid.NewGuid(), questItemBlueprint, corpse);
                    item?.AddBaseItemFlags(false, "StayDeath");
                }
                else
                {
                    Logger.LogError("Loot objective blueprint id {lootBlueprintId} doesn't exist (or is not quest item) for quest {name}", lootBlueprintId, quest.DebugName);
                }
            }
        }
    }
}
