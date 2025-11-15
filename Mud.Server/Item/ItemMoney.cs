using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public class ItemMoney : ItemBase<ItemMoneyBlueprint, ItemData>, IItemMoney
{
    public ItemMoney(Guid guid, ItemMoneyBlueprint blueprint, IContainer containedInto)
        : base(guid, blueprint, containedInto)
    {
        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public ItemMoney(Guid guid, ItemMoneyBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(guid, blueprint, data, containedInto)
    {
        SilverCoins = blueprint.SilverCoins;
        GoldCoins = blueprint.GoldCoins;
    }

    public ItemMoney(Guid guid, ItemMoneyBlueprint blueprint, long silverCoins, long goldCoins, IContainer containedInto)
        : base(guid, blueprint, BuildName(silverCoins, goldCoins), BuildShortDescription(silverCoins, goldCoins), BuildDescription(silverCoins, goldCoins), containedInto)
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
