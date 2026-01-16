using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

public abstract class ItemCastSpellsChargeBase : ItemBase, IItemCastSpellsCharge
{
    protected ItemCastSpellsChargeBase(ILogger<ItemCastSpellsChargeBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

        MaxChargeCount = blueprint.MaxChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = blueprint.AlreadyRecharged;

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
        {
            SpellLevel = RandomManager.Range(blueprint.SpellLevel * 90 / 100, blueprint.SpellLevel * 110 / 100);
            CurrentChargeCount = RandomManager.Fuzzy(blueprint.CurrentChargeCount);
        }
        else
        {
            SpellLevel = blueprint.SpellLevel;
            CurrentChargeCount = blueprint.CurrentChargeCount;
        }
    }

    public void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, ItemCastSpellsChargeData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto);

        SpellLevel = data.SpellLevel;
        MaxChargeCount = data.MaxChargeCount;
        CurrentChargeCount = data.CurrentChargeCount;
        SpellName = blueprint.Spell;
        AlreadyRecharged = data.AlreadyRecharged;
    }

    #region IItemCastSpellsCharge

    public int SpellLevel { get; private set; }
    public int CurrentChargeCount { get; protected set; }
    public int MaxChargeCount { get; protected set; }
    public string? SpellName { get; private set; }
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
