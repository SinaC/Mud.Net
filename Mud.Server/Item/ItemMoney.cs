using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Settings.Interfaces;

namespace Mud.Server.Item;

public class ItemMoney : ItemBase<ItemMoneyBlueprint, ItemData>, IItemMoney
{
    public ItemMoney(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemMoneyBlueprint blueprint, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public ItemMoney(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemMoneyBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public ItemMoney(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemMoneyBlueprint blueprint, long silverCoins, long goldCoins, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, BuildName(silverCoins, goldCoins), BuildShortDescription(silverCoins, goldCoins), BuildDescription(silverCoins, goldCoins), containedInto)
    {
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
