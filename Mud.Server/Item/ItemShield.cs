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

public class ItemShield : ItemBase<ItemShieldBlueprint, ItemData>, IItemShield
{
    public ItemShield(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager,
        Guid guid, ItemShieldBlueprint blueprint, IContainer containedInto) 
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
        Armor = blueprint.Armor;
    }

    public ItemShield(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager,
        Guid guid, ItemShieldBlueprint blueprint, ItemData itemData, IContainer containedInto)
        : base(serviceProvider, gameActionManager, abilityManager, settings, roomManager, auraManager, guid, blueprint, itemData, containedInto)
    {
        Armor = blueprint.Armor;
    }

    #region IItemShield

    public int Armor { get; }

    #endregion
}
