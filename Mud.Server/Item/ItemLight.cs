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

public class ItemLight : ItemBase<ItemLightBlueprint, ItemData>, IItemLight
{
    private const int Infinite = -1;

    public ItemLight(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemLightBlueprint blueprint, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
        TimeLeft = blueprint.DurationHours == Infinite
            ? Infinite
            : blueprint.DurationHours * 60;
    }

    public ItemLight(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemLightBlueprint blueprint, ItemLightData itemData, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, itemData, containedInto)
    {
        TimeLeft = itemData.TimeLeft;
    }

    #region IItemLight

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
