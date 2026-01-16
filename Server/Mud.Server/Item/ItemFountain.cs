using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData.Avatar;
using Mud.Random;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemFountainBlueprint), typeof(ItemData))]
public class ItemFountain : ItemBase, IItemFountain
{
    public ItemFountain(ILogger<ItemFountain> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemFountainBlueprint blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

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

    public string? LiquidName { get; protected set; } = null!;

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
