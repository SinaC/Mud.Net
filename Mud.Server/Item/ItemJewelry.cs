using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

[Export(typeof(IItemJewelry))]
public class ItemJewelry : ItemBase, IItemJewelry
{
    public ItemJewelry(ILogger<ItemJewelry> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    #region IItem

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemJewelry>();

    #endregion

    public override int CarryCount => 0;

    #endregion

    // No additional datas
}
