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

public class ItemTreasure : ItemBase<ItemTreasureBlueprint, ItemData>, IItemTreasure
{
    public ItemTreasure(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemTreasureBlueprint blueprint, IContainer containedInto) 
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
    }

    public ItemTreasure(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemTreasureBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
    }
}
