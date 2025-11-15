using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Item;

public abstract class ItemCastSpellsNoChargeBase<TBlueprint> : ItemBase<TBlueprint, ItemData>, IItemCastSpellsNoCharge
    where TBlueprint : ItemCastSpellsNoChargeBlueprintBase
{
    protected ItemCastSpellsNoChargeBase(Guid guid, TBlueprint blueprint, IContainer containedInto)
        : base(guid, blueprint, containedInto)
    {
        SpellLevel = blueprint.SpellLevel;
        FirstSpellName = blueprint.Spell1;
        SecondSpellName = blueprint.Spell2;
        ThirdSpellName = blueprint.Spell3;
        FourthSpellName = blueprint.Spell4;
    }

    protected ItemCastSpellsNoChargeBase(Guid guid, TBlueprint blueprint, ItemData data, IContainer containedInto)
        : base(guid, blueprint, data, containedInto)
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
