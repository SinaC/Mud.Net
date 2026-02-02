using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemPillBlueprint), typeof(ItemPillData))]
public class ItemPill : ItemCastSpellsNoChargeBase, IItemPill
{
    public ItemPill(ILogger<ItemPill> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    #region IItem

    public override ItemPillData MapItemData()
    {
        return new ItemPillData
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
        };
    }

    #endregion
}
