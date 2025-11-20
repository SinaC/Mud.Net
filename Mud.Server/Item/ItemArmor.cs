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

public class ItemArmor : ItemBase<ItemArmorBlueprint, ItemData>, IItemArmor
{
    public ItemArmor(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto) 
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
        Bash = blueprint.Bash;
        Pierce = blueprint.Pierce;
        Slash = blueprint.Slash;
        Exotic = blueprint.Exotic;
    }

    public ItemArmor(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, ItemArmorBlueprint blueprint, ItemData itemData, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, itemData, containedInto)
    {
        Bash = blueprint.Bash;
        Pierce = blueprint.Pierce;
        Slash = blueprint.Slash;
        Exotic = blueprint.Exotic;
    }

    #region IItemArmor

    public int Bash { get; }
    public int Pierce { get; }
    public int Slash { get; }
    public int Exotic { get; }

    #endregion
}
