using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Item;

[Item(typeof(ItemDrinkContainerBlueprint), typeof(ItemDrinkContainerData))]
public class ItemDrinkContainer : ItemBase, IItemDrinkContainer
{
    public ItemDrinkContainer(ILogger<ItemDrinkContainer> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
            : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemDrinkContainerBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        LiquidName = blueprint.LiquidType;
        MaxLiquid = blueprint.MaxLiquidAmount;
        LiquidLeft = blueprint.CurrentLiquidAmount;
        IsPoisoned = blueprint.IsPoisoned;
    }

    public void Initialize(Guid guid, ItemDrinkContainerBlueprint blueprint, ItemDrinkContainerData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        LiquidName = data.LiquidName;
        MaxLiquid = data.MaxLiquidAmount;
        LiquidLeft = data.CurrentLiquidAmount;
        IsPoisoned = data.IsPoisoned;
    }

    #region IItemDrinkContainer

    #region IItemDrinkable

    #region IItemPoisonable

    public bool IsPoisoned { get; protected set; }

    public void Poison()
    {
        IsPoisoned = true;
    }

    public void Cure()
    {
        IsPoisoned = false;
    }

    #endregion

    public string? LiquidName { get; protected set; } = null!;

    public int LiquidLeft { get; protected set; }

    public bool IsEmpty => LiquidLeft <= 0;


    public int LiquidAmountMultiplier => 1;

    public void Drink(int amount)
    {
        LiquidLeft = Math.Max(0, LiquidLeft - amount);
    }

    #endregion

    public int MaxLiquid { get; protected set; }

    public void Fill(string? liquidName, int amount)
    {
        LiquidName = liquidName;
        LiquidLeft = Math.Min(MaxLiquid, LiquidLeft + amount);
    }

    public void Fill(int amount)
    {
        LiquidLeft = Math.Min(MaxLiquid, LiquidLeft + amount);
    }

    public void Pour()
    {
        LiquidLeft = 0;
        IsPoisoned = false;
    }

    #endregion

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemDrinkContainer>();

    #endregion

    #region ItemBase

    public override ItemDrinkContainerData MapItemData()
    {
        return new ItemDrinkContainerData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            MaxLiquidAmount = MaxLiquid,
            CurrentLiquidAmount = LiquidLeft,
            LiquidName = LiquidName,
            IsPoisoned = IsPoisoned,
            Auras = MapAuraData(),
        };
    }

#endregion
}
