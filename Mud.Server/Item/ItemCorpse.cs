using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Item;

[Item(typeof(ItemCorpseBlueprint), typeof(ItemCorpseData))]
public class ItemCorpse : ItemBase, IItemCorpse
{
    private readonly List<IItem> _content;

    private IRandomManager RandomManager { get; }
    private IItemManager ItemManager { get; }

    private string CorpseName { get; set; } = null!;
    private bool HasBeenGeneratedByKillingCharacter { get; set; }

    public ItemCorpse(ILogger<ItemCorpse> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, IRandomManager randomManager, IItemManager itemManager)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
        RandomManager = randomManager;
        ItemManager = itemManager;

        _content = [];
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IContainer container)
    {
        base.Initialize(guid, blueprint, container);

        CorpseName = null!;
        HasBeenGeneratedByKillingCharacter = false;
        IsPlayableCharacterCorpse = false;
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, ItemCorpseData itemCorpseData, IContainer container)
    {
        base.Initialize(guid, blueprint, itemCorpseData, BuildName(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildShortDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), container);

        CorpseName = itemCorpseData.CorpseName;
        HasBeenGeneratedByKillingCharacter = itemCorpseData.HasBeenGeneratedByKillingCharacter;

        IsPlayableCharacterCorpse = itemCorpseData.IsPlayableCharacterCorpse;
        if (itemCorpseData.Contains?.Length > 0)
        {
            foreach (ItemData itemData in itemCorpseData.Contains)
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
        }
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
    {
        Initialize(guid, blueprint, BuildName(victim.DisplayName, true, blueprint), BuildShortDescription(victim.DisplayName, true, blueprint), BuildDescription(victim.DisplayName, true, blueprint), room);

        CorpseName = victim.DisplayName;
        HasBeenGeneratedByKillingCharacter = true;

        IsPlayableCharacterCorpse = victim is IPlayableCharacter;

        if (IsPlayableCharacterCorpse)
        {
            DecayPulseLeft = RandomManager.Range(25, 40) * Pulse.PulsePerMinutes;
            BaseItemFlags.Set("NoPurge"); // was handled in object description in limbo.are
            NoTake = true;
        }
        else
        {
            DecayPulseLeft = RandomManager.Range(3, 6) * Pulse.PulsePerMinutes;
            NoTake = false;
        }

        // Money
        if (victim.SilverCoins > 0 || victim.GoldCoins > 0)
        {
            long silver = victim.SilverCoins;
            long gold = victim.GoldCoins;
            if ((silver > 1 || gold > 1)
                && victim is IPlayableCharacter pcVictim) // player keep half their money
            {
                silver /= 2;
                gold /= 2;
                pcVictim.UpdateMoney(-silver, -gold);
            }

            ItemManager.AddItemMoney(Guid.NewGuid(), silver, gold, this);
        }

        // Fill corpse with inventory
        var inventory = victim.Inventory.ToArray();
        foreach (var item in inventory)
        {
            var result = PerformActionOnItem(victim, item);
            if (result == PerformActionOnItemResults.MoveToCorpse)
                item.ChangeContainer(this);
            else if (result == PerformActionOnItemResults.MoveToRoom)
                item.ChangeContainer(victim.Room);
            else
                ItemManager.RemoveItem(item);
        }
        // Fill corpse with equipment
        var equipment = victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!).ToArray();
        foreach (var item in equipment)
        {
            var result = PerformActionOnItem(victim, item);
            if (result == PerformActionOnItemResults.MoveToCorpse)
            {
                item.ChangeEquippedBy(null, false);
                item.ChangeContainer(this);
            }
            else if (result == PerformActionOnItemResults.MoveToRoom)
            {
                item.ChangeEquippedBy(null, false);
                item.ChangeContainer(victim.Room);
            }
            else
                ItemManager.RemoveItem(item);
        }

        // Check victim loot table (only if victim is NPC)
        if (victim is INonPlayableCharacter npcVictim)
        {
            var loots = npcVictim.Blueprint?.LootTable?.GenerateLoots();
            if (loots != null && loots.Count != 0)
            {
                foreach (int loot in loots)
                    ItemManager.AddItem(Guid.NewGuid(), loot, this);
            }
        }
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer)
    {
        Initialize(guid, blueprint, room, victim);

        // Check killer quest table (only if killer is PC and victim is NPC) // TODO: only visible for people on quest???
        if (killer != null && killer is IPlayableCharacter playableCharacterKiller && victim is INonPlayableCharacter nonPlayableCharacterVictim)
        {
            foreach (IQuest quest in playableCharacterKiller.Quests)
            {
                // Update kill objectives
                quest.Update(nonPlayableCharacterVictim);
                // Generate loot on corpse
                quest.GenerateKillLoot(nonPlayableCharacterVictim, this);
            }
        }
    }

    #region IItemCorpse

    #region IContainer

    public IEnumerable<IItem> Content
        => _content;

    public bool PutInContainer(IItem obj)
    {
        _content.Add(obj);
        return true;
    }

    public bool GetFromContainer(IItem obj)
    {
        return _content.Remove(obj);
    }

    #endregion

    public bool IsPlayableCharacterCorpse { get; protected set; }

    #endregion

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemCorpse>();

    #endregion

    #region ItemBase

    public override ItemData MapItemData()
    {
        return new ItemCorpseData
        {
            ItemId = Blueprint.Id,
            CorpseName = CorpseName,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            Contains = MapContent(),
            IsPlayableCharacterCorpse = IsPlayableCharacterCorpse,
            HasBeenGeneratedByKillingCharacter = HasBeenGeneratedByKillingCharacter,
        };
    }

    #endregion

    private static string BuildName(string corpseName, bool generated, ItemCorpseBlueprint blueprint) 
        => generated
            ? "corpse " + corpseName
            : blueprint.Name;
    private static string BuildShortDescription(string corpseName, bool generated, ItemCorpseBlueprint blueprint)
        => generated
            ? "the corpse of " + corpseName
            : blueprint.ShortDescription;
    private static string BuildDescription(string corpseName, bool generated, ItemCorpseBlueprint blueprint)
        => generated
            ? $"The corpse of {corpseName} is lying here."
            : blueprint.Description;

    // Perform actions on item before putting it in corpse
    // returns false if item must be destroyed instead of being put in corpse
    public enum PerformActionOnItemResults
    {
        MoveToCorpse,
        MoveToRoom,
        Destroy,
    }
    private PerformActionOnItemResults PerformActionOnItem(ICharacter victim, IItem item)
    {
        if (item.ItemFlags.IsSet("Inventory"))
            return PerformActionOnItemResults.Destroy;
        // TODO: check stay death flag
        if (item is IItemPotion)
            item.SetTimer(TimeSpan.FromMinutes(RandomManager.Range(500,1000)));
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
                        contentItem.ChangeContainer(victim.Room);
                }
                else
                    victim.Act(ActOptions.ToRoom, "{0} evaporates.", item);
                return PerformActionOnItemResults.Destroy;
            }
            victim.Act(ActOptions.ToRoom, "{0} falls to the floor.", item);
            item.Recompute();
            return PerformActionOnItemResults.MoveToRoom;
        }
        if (item.ItemFlags.IsSet("RotDeath"))
        {
            int duration = RandomManager.Range(5, 10);
            item.SetTimer(TimeSpan.FromMinutes(duration));
            item.RemoveBaseItemFlags(false, "RotDeath");
        }
        item.Recompute();
        return PerformActionOnItemResults.MoveToCorpse;
    }

    private ItemData[] MapContent()
    {
        if (Content.Any())
            return Content.Select(x => x.MapItemData()).ToArray();
        return [];
    }
}
