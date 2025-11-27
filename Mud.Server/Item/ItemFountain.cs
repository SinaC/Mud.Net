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

[Export(typeof(IItemFountain))]
public class ItemFountain : ItemBase, IItemFountain
{
    public ItemFountain(ILogger<ItemFountain> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemFountainBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        LiquidName = blueprint.LiquidType;
    }

    public void Initialize(Guid guid, ItemFountainBlueprint blueprint, ItemData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        LiquidName = blueprint.LiquidType;
    }

    #region IItemFountain

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemFountain>();

    #endregion

    #region IItemDrinkable

    public string LiquidName { get; protected set; } = null!;

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
