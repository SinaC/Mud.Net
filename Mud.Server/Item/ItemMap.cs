using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

public class ItemMap : ItemBase<ItemMapBlueprint, ItemData>, IItemMap
{
    public ItemMap(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemMapBlueprint blueprint, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
    }

    public ItemMap(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemMapBlueprint blueprint, ItemData data, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
    }
}
