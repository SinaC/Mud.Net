using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Random;

namespace Mud.Server.Item;

public abstract class ItemCastSpellsNoChargeBase : ItemBase, IItemCastSpellsNoCharge
{
    protected ItemCastSpellsNoChargeBase(ILogger<ItemCastSpellsNoChargeBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IRoomManager roomManager, IAuraManager auraManager)
    : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
            SpellLevel = RandomManager.Range(blueprint.SpellLevel * 90 / 100, blueprint.SpellLevel * 110 / 100);
        else
            SpellLevel = blueprint.SpellLevel;
    }

    public void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, ItemCastSpellsNoChargeData data, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, data, containedInto); 

        SpellLevel = blueprint.SpellLevel;
        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;
    }

    #region IItemCastSpellsNoCharge

    public int SpellLevel { get; private set; }

    public string? FirstSpellName { get; private set; }

    public string? SecondSpellName { get; private set; }

    public string? ThirdSpellName { get; private set; }

    public string? FourthSpellName { get; private set; }

    #endregion
}
