using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.SerializationData;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Options;

namespace Mud.Server.Item;

public abstract class ItemCastSpellsNoChargeBase : ItemBase, IItemCastSpellsNoCharge
{
    protected ItemCastSpellsNoChargeBase(ILogger<ItemCastSpellsNoChargeBase> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
    : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, string source, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, source, containedInto);

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

        SpellLevel = data.SpellLevel;
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
