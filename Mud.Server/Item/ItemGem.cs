using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.DataStructures.Trie;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item;

[Item(typeof(ItemGemBlueprint), typeof(ItemData))]
public class ItemGem : ItemBase, IItemGem
{
    public ItemGem(ILogger<ItemGem> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemGem>();

    #endregion
}
