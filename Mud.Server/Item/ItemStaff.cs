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

public class ItemStaff : ItemCastSpellsChargeBase<ItemStaffBlueprint, ItemStaffData>, IItemStaff
{
    public ItemStaff(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemStaffBlueprint blueprint, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
    }

    public ItemStaff(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemStaffBlueprint blueprint, ItemStaffData data, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
    }

    #region ItemBase

    public override ItemData MapItemData()
    {
        return new ItemStaffData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags,
            Auras = MapAuraData(),
            MaxChargeCount = MaxChargeCount,
            CurrentChargeCount = CurrentChargeCount,
            AlreadyRecharged = AlreadyRecharged
        };
    }

    #endregion
}
