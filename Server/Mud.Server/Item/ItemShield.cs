using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Commands.Admin.Administration;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Random;

namespace Mud.Server.Item;

[Item(typeof(ItemShieldBlueprint), typeof(ItemShieldData))]
public class ItemShield : ItemBase, IItemShield
{
    public ItemShield(ILogger<ItemShield> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
            Armor = RandomManager.Range(blueprint.Armor * 95 / 100, blueprint.Armor * 105 / 100);
        else
            Armor = blueprint.Armor;
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, ItemShieldData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Armor = itemData.Armor;
    }

    #region IItemShield

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemShield>();

    #endregion

    public int Armor { get; private set; }

    #region ItemBase

    public override ItemShieldData MapItemData()
    {
        return new ItemShieldData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            Armor = Armor
        };
    }

    #endregion

    #endregion
}
