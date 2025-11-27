using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Export(typeof(IItemMoney))]
public class ItemMoney : ItemBase, IItemMoney
{
    public ItemMoney(ILogger<ItemMoney> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, ItemData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, long silverCoins, long goldCoins, IContainer containedInto)
    {
        Initialize(guid, blueprint, BuildName(silverCoins, goldCoins), BuildShortDescription(silverCoins, goldCoins), BuildDescription(silverCoins, goldCoins), containedInto);

        SilverCoins = silverCoins;
        GoldCoins = goldCoins;
    }

    #region IItemMoney

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemMoney>();

    #endregion

    public long SilverCoins { get; protected set; }

    public long GoldCoins { get; protected set; }

    #endregion

    private static string BuildName(long silver, long gold)
    {
        if (silver == 1 && gold == 0)
            return "coin silver gcash";
        if (silver == 0 && gold == 1)
            return "coin gold gcash";
        if (silver > 1 && gold == 0)
            return "coins silver gcash";
        if (silver == 0 && gold > 1)
            return "coins gold gcash";
        return "coins silver gold gcash";
    }

    private static string BuildShortDescription(long silver, long gold)
    {
        if (silver == 1 && gold == 0)
            return "a silver coin";
        if (silver == 0 && gold == 1)
            return "a gold coin";
        if (silver > 1 && gold == 0)
            return $"{silver} silver coins";
        if (silver == 0 && gold > 1)
            return $"{gold} gold coins";
        return $"{silver} silver coins and {gold} gold coins";
    }

    private static string BuildDescription(long silver, long gold)
    {
        if (silver == 1 && gold == 0)
            return "One miserable coin is here.";
        if (silver == 0 && gold == 1)
            return "One valuable gold coin.";
        if (silver > 1 && gold == 0)
            return "A pile of silver coins.";
        if (silver == 0 && gold > 1)
            return "A pile of gold coins.";
        return "A pile of coins.";
    }
}
