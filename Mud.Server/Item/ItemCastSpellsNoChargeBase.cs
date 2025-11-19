using Microsoft.Extensions.Logging;
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

public abstract class ItemCastSpellsNoChargeBase<TBlueprint> : ItemBase<TBlueprint, ItemData>, IItemCastSpellsNoCharge
    where TBlueprint : ItemCastSpellsNoChargeBlueprintBase
{
    protected ItemCastSpellsNoChargeBase(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, TBlueprint blueprint, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, containedInto)
    {
        SpellLevel = blueprint.SpellLevel;
        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;
    }

    protected ItemCastSpellsNoChargeBase(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, 
        Guid guid, TBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, settings, roomManager, auraManager, guid, blueprint, data, containedInto)
    {
        SpellLevel = blueprint.SpellLevel;
        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;
    }

    #region IItemCastSpellsNoCharge

    public int SpellLevel { get; }

    public string FirstSpellName { get; }

    public string SecondSpellName { get; }

    public string ThirdSpellName { get; }

    public string FourthSpellName { get; }

    #endregion
}
