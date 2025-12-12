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

[Item(typeof(ItemShieldBlueprint), typeof(ItemData))]
public class ItemShield : ItemBase, IItemShield
{
    public ItemShield(ILogger<ItemShield> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, itemFlagFactory)
    {
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        Armor = blueprint.Armor;
    }

    public void Initialize(Guid guid, ItemShieldBlueprint blueprint, ItemData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Armor = blueprint.Armor;
    }

    #region IItemShield

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemShield>();

    #endregion

    public int Armor { get; private set; }

    #endregion
}
