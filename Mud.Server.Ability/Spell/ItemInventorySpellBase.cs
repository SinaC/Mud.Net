using Microsoft.Extensions.Logging;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class ItemInventorySpellBase : SpellBase, ITargetedAction
{
    protected ItemInventorySpellBase(ILogger<ItemInventorySpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Inventory.Where(caster.CanSee);

    protected IItem Item { get; set; } = default!;

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            if (spellActionInput.CastFromItemOptions.PredefinedTarget is not IItem item)
                return "You can't do that.";
            Item = item;
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
            return "What should it be used upon?";
        Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0])!; // TODO: equipments ?
        if (Item == null)
            return "You are not carrying that.";
        return null;
    }
}

public abstract class ItemInventorySpellBase<TItem> : SpellBase, ITargetedAction
    where TItem : class, IItem
{
    protected TItem Item { get; set; } = default!;

    protected ItemInventorySpellBase(ILogger<ItemInventorySpellBase<TItem>> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Inventory.OfType<TItem>().Where(caster.CanSee);

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            if (spellActionInput.CastFromItemOptions.PredefinedTarget is not TItem item)
                return "You can't do that.";
            Item = item;
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
            return IsCastFromItem
                ? "What should it be used upon?"
                : "What should the spell be cast upon?";
        var target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
        if (target == null)
            return "You are not carrying that.";
        if (target is not TItem targetItem)
            return InvalidItemTypeMsg;
        Item = targetItem;
        return null;
    }

    protected abstract string InvalidItemTypeMsg { get; }
}
