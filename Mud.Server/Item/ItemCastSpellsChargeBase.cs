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

public abstract class ItemCastSpellsChargeBase<TBlueprint, TData> : ItemBase<TBlueprint, TData>, IItemCastSpellsCharge
    where TBlueprint : ItemCastSpellsChargeBlueprintBase
    where TData: ItemCastSpellsChargeData
{
    protected ItemCastSpellsChargeBase(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, TBlueprint blueprint, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, containedInto)
    {
        SpellLevel = blueprint.SpellLevel;
        MaxChargeCount = blueprint.MaxChargeCount;
        CurrentChargeCount = blueprint.CurrentChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = blueprint.AlreadyRecharged;
    }

    protected ItemCastSpellsChargeBase(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
        SpellLevel = blueprint.SpellLevel;
        MaxChargeCount = data.MaxChargeCount;
        CurrentChargeCount = data.CurrentChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = data.AlreadyRecharged;
    }

    #region IItemCastSpellsCharge

    public int SpellLevel { get; }
    public int CurrentChargeCount { get; protected set; }
    public int MaxChargeCount { get; protected set; }
    public string SpellName { get; }
    public bool AlreadyRecharged { get; protected set; }

    public void Use()
    {
        CurrentChargeCount = Math.Max(0, CurrentChargeCount - 1);
    }

    public void Recharge(int currentChargeCount, int maxChargeCount)
    {
        AlreadyRecharged = true;
        MaxChargeCount = maxChargeCount;
        CurrentChargeCount = Math.Min(currentChargeCount, MaxChargeCount);
    }

    #endregion
}
