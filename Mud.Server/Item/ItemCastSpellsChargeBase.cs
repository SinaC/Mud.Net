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

public abstract class ItemCastSpellsChargeBase : ItemBase, IItemCastSpellsCharge
{
    protected ItemCastSpellsChargeBase(ILogger<ItemCastSpellsChargeBase> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        SpellLevel = blueprint.SpellLevel;
        MaxChargeCount = blueprint.MaxChargeCount;
        CurrentChargeCount = blueprint.CurrentChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = blueprint.AlreadyRecharged;
    }

    public void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, ItemCastSpellsChargeData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        SpellLevel = blueprint.SpellLevel;
        MaxChargeCount = data.MaxChargeCount;
        CurrentChargeCount = data.CurrentChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = data.AlreadyRecharged;
    }

    #region IItemCastSpellsCharge

    public int SpellLevel { get; private set; }
    public int CurrentChargeCount { get; protected set; }
    public int MaxChargeCount { get; protected set; }
    public string SpellName { get; private set; } = null!;
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

    #region IItem

    public override ItemData MapItemData()
    {
        return new ItemWandData
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

    #endregion
}
