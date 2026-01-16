using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Random;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemWandBlueprint), typeof(ItemWandData))]
public class ItemWand : ItemCastSpellsChargeBase, IItemWand
{
    public ItemWand(ILogger<ItemWand> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemWand>();

    #endregion

    #region IItem

    public override ItemWandData MapItemData()
    {
        return new ItemWandData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            Source = Source,
            SpellLevel = SpellLevel,
            MaxChargeCount = MaxChargeCount,
            CurrentChargeCount = CurrentChargeCount,
            AlreadyRecharged = AlreadyRecharged
        };
    }

    #endregion
}
