using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item.SerializationData;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Item;

[Item(typeof(ItemCorpseBlueprint), typeof(ItemCorpseData))]
public class ItemCorpse : ItemBase, IItemCorpse
{
    private string _corpseName = null!;
    private bool _hasBeenGeneratedByKillingCharacter;
    private readonly List<IItem> _content;

    private IRandomManager RandomManager { get; }
    private IItemManager ItemManager { get; }

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

        _corpseName = null!;
        _hasBeenGeneratedByKillingCharacter = false;
        IsPlayableCharacterCorpse = false;
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, ItemCorpseData itemCorpseData, IContainer container)
    {
        base.Initialize(guid, blueprint, itemCorpseData, BuildName(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildShortDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), BuildDescription(itemCorpseData.CorpseName, itemCorpseData.HasBeenGeneratedByKillingCharacter, blueprint), container);

        _corpseName = itemCorpseData.CorpseName;
        _hasBeenGeneratedByKillingCharacter = itemCorpseData.HasBeenGeneratedByKillingCharacter;

        IsPlayableCharacterCorpse = itemCorpseData.IsPlayableCharacterCorpse;
        if (itemCorpseData.Contains?.Length > 0)
        {
            foreach (var itemData in itemCorpseData.Contains)
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
        }
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
    {
        Initialize(guid, blueprint, BuildName(victim.DisplayName, true, blueprint), BuildShortDescription(victim.DisplayName, true, blueprint), BuildDescription(victim.DisplayName, true, blueprint), room);

        _corpseName = victim.DisplayName;
        _hasBeenGeneratedByKillingCharacter = true;

        IsPlayableCharacterCorpse = victim is IPlayableCharacter;

        if (IsPlayableCharacterCorpse)
        {
            DecayPulseLeft = Pulse.FromMinutes(RandomManager.Range(25, 40));
            BaseItemFlags.Set("NoPurge"); // was handled in object description in limbo.are
            NoTake = true;
        }
        else
        {
            DecayPulseLeft = Pulse.FromMinutes(RandomManager.Range(3, 6));
            NoTake = false;
        }
    }

    public void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, IEnumerable<IPlayableCharacter> playableCharactersImpactedByKill)
    {
        Initialize(guid, blueprint, room, victim);

        if (victim is INonPlayableCharacter npcVictim)
        {
            // Check killer quest table (only if killer is PC and victim is NPC) // TODO: only visible for people on quest???
            foreach (var playableCharacterImpactedByKill in playableCharactersImpactedByKill)
            {
                foreach (var quest in playableCharacterImpactedByKill.ActiveQuests)
                {
                    // Update kill objectives
                    quest.Update(npcVictim);
                    // Generate loot on corpse
                    quest.GenerateKillLoot(npcVictim, this);
                }
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
            CorpseName = _corpseName,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            Contains = MapContent(),
            IsPlayableCharacterCorpse = IsPlayableCharacterCorpse,
            HasBeenGeneratedByKillingCharacter = _hasBeenGeneratedByKillingCharacter,
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

    private ItemData[] MapContent()
    {
        if (Content.Any())
            return Content.Select(x => x.MapItemData()).ToArray();
        return [];
    }
}
