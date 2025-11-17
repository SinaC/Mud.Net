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

public class ItemFountain : ItemBase<ItemFountainBlueprint, ItemData>, IItemFountain
{
    public ItemFountain(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemFountainBlueprint blueprint, IContainer containedInto)
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
        LiquidName = blueprint.LiquidType;
    }

    public ItemFountain(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemFountainBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
        LiquidName = blueprint.LiquidType;
    }

    #region IItemFountain

    #region IItemDrinkable

    public string LiquidName { get; protected set; }

    public int LiquidLeft => int.MaxValue;

    public bool IsEmpty => false;

    public int LiquidAmountMultiplier => 3;

    public void Drink(int amount)
    {
        // NOP: infinite container
    }

    #endregion

    #endregion
}
