using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class ItemOrDefensiveSpellBase : SpellBase, ITargetedAction
{
    protected ItemOrDefensiveSpellBase(ILogger<ItemOrDefensiveSpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IEntity Target { get; set; } = default!;

    public IEnumerable<IEntity> ValidTargets(ICharacter caster)
        =>
        caster.Room.People.Where(caster.CanSee).OfType<IEntity>().Concat(caster.Inventory.Where(caster.CanSee));

    protected override void Invoke()
    {
        if (Target is IItem item)
            Invoke(item);
        if (Target is ICharacter victim)
            Invoke(victim);
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            Target = (spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter) ?? ((spellActionInput.CastFromItemOptions.PredefinedTarget as IItem) as IEntity) ?? Caster;
            return null;
        }

        Target = spellActionInput.Parameters.Length < 1
                    ? Caster
                    : FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0])!;
        if (Target == null)
        {
            Target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0])!;
            if (Target == null)
                return "You don't see that here.";
        }
        // victim or item (target) found
        return null;
    }

    protected abstract void Invoke(ICharacter victim);
    protected abstract void Invoke(IItem item);
}
