using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

public class ItemFountain : ItemBase<ItemFountainBlueprint, ItemData>, IItemFountain
{
    public ItemFountain(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemFountainBlueprint blueprint, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
        LiquidName = blueprint.LiquidType;
    }

    public ItemFountain(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemFountainBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, data, containedInto)
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
