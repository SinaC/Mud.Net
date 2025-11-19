using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Settings.Interfaces;

namespace Mud.Server.Item;

public class ItemBoat : ItemBase<ItemBoatBlueprint, ItemData>, IItemBoat
{
    public ItemBoat(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemBoatBlueprint blueprint, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
    }

    public ItemBoat(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemBoatBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
    }
}
