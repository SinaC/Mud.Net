using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Ability.Spell;

public abstract class OptionalItemInventorySpellBase : SpellBase
{
    protected OptionalItemInventorySpellBase(ILogger<OptionalItemInventorySpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IItem Item { get; set; } = default!;


    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            Item = (spellActionInput.CastFromItemOptions.PredefinedTarget as IItem) ?? default!;
            return null;
        }

        Item = default!; // TODO: is this necessary ?
        if (spellActionInput.Parameters.Length >= 1)
        {
            Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0])!; // TODO: equipments ?
            if (Item == null)
                return "You are not carrying that.";
        }
        return null;
    }
}
