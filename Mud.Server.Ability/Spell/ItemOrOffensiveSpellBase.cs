using Microsoft.Extensions.Logging;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Ability.Spell;

public abstract class ItemOrOffensiveSpellBase : SpellBase, ITargetedAction
{
    protected ItemOrOffensiveSpellBase(ILogger<ItemOrOffensiveSpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IEntity Target { get; set; } = default!;

    public override void Execute()
    {
        base.Execute();

        // multi hit if still in same room
        var npcVictim = Target as INonPlayableCharacter;
        if (Target != Caster
            && npcVictim?.Master != Caster)
        {
            // TODO: not sure why we loop on people in caster room
            // TODO: we could just check if victim is still in the room and not fighting
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.ToList());
            foreach (ICharacter victim in clone)
            {
                if (victim == Target && victim.Fighting == null)
                {
                    // TODO: check_killer
                    victim.MultiHit(Caster);
                    break;
                }
            }
        }
    }

    public IEnumerable<IEntity> ValidTargets(ICharacter caster)
        =>
        caster.Room.People.Where(x => x != caster && caster.CanSee(x) && IsTargetValid(caster, x)).OfType<IEntity>()
        .Concat(caster.Room.Content.Where(caster.CanSee)).OfType<IEntity>()
        .Concat(caster.Inventory.Where(caster.CanSee)).OfType<IEntity>()
        .Concat(caster.Equipments.Where(x => x.Item != null && caster.CanSee(x.Item)).Select(x => x.Item)).OfType<IEntity>();

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
            Target = (spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter) ?? ((spellActionInput.CastFromItemOptions.PredefinedTarget as IItem) as IEntity) ?? Caster.Fighting!;
            if (Target == null)
                return "You can't do that.";
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
        {
            Target = Caster.Fighting!;
            if (Target == null)
                return IsCastFromItem
                    ? "Use it on whom or what?"
                    : "Cast the spell on whom or what?";
        }
        else
            Target = FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0])!;
        if (Target != null)
        {
            if (Caster is IPlayableCharacter)
            {
                if (Caster != Target)
                {
                    var safeResult = ((ICharacter)Target).IsSafe(Caster);
                    if (safeResult != null)
                        return safeResult;
                }
                // TODO: check_killer
            }
            if (Caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.IsSet("Charm") && npcCaster.Master == Target)
                return "You can't do that on your own follower.";
        }
        else // character not found, search item in room, in inventor, in equipment
        {
            Target = FindHelpers.FindItemHere(Caster, spellActionInput.Parameters[0])!;
            if (Target == null)
                return "You don't see that here.";
        }
        // victim or item (target) found
        return null;
    }

    protected abstract void Invoke(ICharacter victim);
    protected abstract void Invoke(IItem item);

    private static bool IsTargetValid(ICharacter caster, ICharacter victim)
    {
        if (caster is IPlayableCharacter)
        {
            if (caster != victim && victim.IsSafe(caster) != null)
                return false;
        }
        if (caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.IsSet("Charm") && npcCaster.Master == victim)
            return false;
        return true;
    }
}
