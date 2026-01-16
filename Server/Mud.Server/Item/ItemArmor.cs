using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.DataStructures.Trie;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Random;

namespace Mud.Server.Item;

[Item(typeof(ItemArmorBlueprint), typeof(ItemArmorData))]
public class ItemArmor : ItemBase, IItemArmor
{
    public ItemArmor(ILogger<ItemArmor> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
        {
            Bash = RandomManager.Range(blueprint.Bash * 95 / 100, blueprint.Bash * 105 / 100);
            Pierce = RandomManager.Range(blueprint.Pierce * 95 / 100, blueprint.Pierce * 105 / 100);
            Slash = RandomManager.Range(blueprint.Slash * 95 / 100, blueprint.Slash * 105 / 100);
            Exotic = RandomManager.Range(blueprint.Exotic * 95 / 100, blueprint.Exotic * 105 / 100);
        }
        else
        {
            Bash = blueprint.Bash;
            Pierce = blueprint.Pierce;
            Slash = blueprint.Slash;
            Exotic = blueprint.Exotic;
        }
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, ItemArmorData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Bash = itemData.Bash;
        Pierce = itemData.Pierce;
        Slash = itemData.Slash;
        Exotic = itemData.Exotic;
    }

    #region IItemArmor

    public int Bash { get; private set; }
    public int Pierce { get; private set; }
    public int Slash { get; private set; }
    public int Exotic { get; private set; }

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemArmor>();

    #endregion

    #region ItemBase

    public override ItemArmorData MapItemData()
    {
        return new ItemArmorData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            Bash = Bash,
            Pierce = Pierce,
            Slash = Slash,
            Exotic = Exotic,
        };
    }

    #endregion

    #endregion
}
