using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemWandBlueprint), typeof(ItemWandData))]
public class ItemWand : ItemCastSpellsChargeBase, IItemWand
{
    public ItemWand(ILogger<ItemWand> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    #region IItem

    public override ItemWandData MapItemData()
    {
        return new ItemWandData
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
            SpellLevel = SpellLevel,
            MaxChargeCount = MaxChargeCount,
            CurrentChargeCount = CurrentChargeCount,
            AlreadyRecharged = AlreadyRecharged
        };
    }

    #endregion
}
