using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Item;

public abstract class ItemCastSpellsNoChargeBase : ItemBase, IItemCastSpellsNoCharge
{
    protected ItemCastSpellsNoChargeBase(ILogger<ItemCastSpellsNoChargeBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory)
    : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager, itemFlagFactory)
    {
    }

    public void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, containedInto);

        SpellLevel = blueprint.SpellLevel;
        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;
    }

    public void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, ItemData data, IContainer containedInto)
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

    public string FirstSpellName { get; private set; } = null!;

    public string SecondSpellName { get; private set; } = null!;

    public string ThirdSpellName { get; private set; } = null!;

    public string FourthSpellName { get; private set; } = null!;

    #endregion
}
