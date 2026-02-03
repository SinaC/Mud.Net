using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain.SerializationData.Avatar;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemMoneyBlueprint), typeof(ItemData))]
public class ItemMoney : ItemBase, IItemMoney
{
    public ItemMoney(ILogger<ItemMoney> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, ItemData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public void Initialize(Guid guid, ItemMoneyBlueprint blueprint, long silverCoins, long goldCoins, string source, IContainer containedInto)
    {
        Initialize(guid, blueprint, BuildName(silverCoins, goldCoins), BuildShortDescription(silverCoins, goldCoins), BuildDescription(silverCoins, goldCoins), source, containedInto);

        SilverCoins = silverCoins;
        GoldCoins = goldCoins;
    }

    #region IItemMoney

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
