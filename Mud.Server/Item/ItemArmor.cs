using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Item(typeof(ItemArmorBlueprint), typeof(ItemData))]
public class ItemArmor : ItemBase, IItemArmor
{
    public ItemArmor(ILogger<ItemArmor> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, itemFlagFactory)
    {
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        Bash = blueprint.Bash;
        Pierce = blueprint.Pierce;
        Slash = blueprint.Slash;
        Exotic = blueprint.Exotic;
    }

    public void Initialize(Guid guid, ItemArmorBlueprint blueprint, ItemData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Bash = blueprint.Bash;
        Pierce = blueprint.Pierce;
        Slash = blueprint.Slash;
        Exotic = blueprint.Exotic;
    }

    #region IItemArmor

    public int Bash { get; private set; }
    public int Pierce { get; private set; }
    public int Slash { get; private set; }
    public int Exotic { get; private set; }

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemArmor>();

    #endregion

    #endregion
}
