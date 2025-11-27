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

[Export(typeof(IItemLight))]
public class ItemLight : ItemBase, IItemLight
{
    private const int Infinite = -1;

    public ItemLight(ILogger<ItemLight> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
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

    public override ItemData MapItemData()
    {
        return new ItemLightData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags,
            Auras = MapAuraData(),
            TimeLeft = TimeLeft,
        };
    }

    #endregion
}
