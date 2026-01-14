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

[Item(typeof(ItemLightBlueprint), typeof(ItemLightData))]
public class ItemLight : ItemBase, IItemLight
{
    private const int Infinite = -1;

    public ItemLight(ILogger<ItemLight> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        TimeLeft = blueprint.DurationHours == Infinite
            ? Infinite
            : blueprint.DurationHours * 60;
    }

    public void Initialize(Guid guid, ItemLightBlueprint blueprint, ItemLightData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        TimeLeft = itemData.TimeLeft;
    }

    #region IItemLight

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemLight>();

    #endregion

    public bool IsLighten => TimeLeft == Infinite || TimeLeft > 0;

    public int TimeLeft { get; protected set; }

    public bool IsInfinite => TimeLeft == Infinite;

    public bool DecreaseTimeLeft()
    {
        if (TimeLeft != Infinite && TimeLeft > 0)
            TimeLeft--;
        return TimeLeft == 0;
    }

    #endregion

    #region ItemBase

    public override ItemLightData MapItemData()
    {
        return new ItemLightData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            TimeLeft = TimeLeft,
        };
    }

    #endregion
}
