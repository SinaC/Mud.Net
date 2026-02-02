using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemLightBlueprint), typeof(ItemLightData))]
public class ItemLight : ItemBase, IItemLight
{
    private const int Infinite = -1;

    public ItemLight(ILogger<ItemLight> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemLightBlueprint blueprint, string source, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, source, containedInto);

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
            Source = Source,
            ShortDescription = ShortDescription,
            Description = Description,
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
