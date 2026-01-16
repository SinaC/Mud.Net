using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Random;

namespace Mud.Server.Ability.Spell;

public abstract class OffensiveSpellBase : SpellBase, ITargetedAction
{
    protected OffensiveSpellBase(ILogger<OffensiveSpellBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected ICharacter Victim { get; set; } = default!;

    public override void Execute()
    {
        base.Execute();

        var npcVictim = Victim as INonPlayableCharacter;
        if (Victim != Caster
            && npcVictim?.Master != Caster) // avoid attacking caster when successfully charmed
        {
            // check if victim is still in the room and not yet fighting user
            // if victim found, we allow it to multi hit user
            var victimStartFightaingAgainstUser = Caster.Room.People.FirstOrDefault(x => x == Victim && x.Fighting == null);
            if (victimStartFightaingAgainstUser != null)
            {
                // TODO: check_killer
                victimStartFightaingAgainstUser.MultiHit(Caster);
            }
        }
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter ?? Caster.Fighting!;
            if (Victim == null)
                return "You can't do that.";
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
        {
            Victim = Caster.Fighting!;
            if (Victim == null)
                return IsCastFromItem
                    ? "Use it on whom?"
                    : "Cast the spell on whom?";
        }
        else
            Victim = FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee).OfType<ICharacter>(), spellActionInput.Parameters[0])!; // original code was not testing CanSee
        if (Victim == null)
            return StringHelpers.CharacterNotFound;
        if (Caster is IPlayableCharacter)
        {
            if (Caster != Victim)
            {
                var safeResult = Victim.IsSafe(Caster);
                if (safeResult != null)
                    return safeResult;
            }
            // TODO: check_killer
        }
        if (Caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.IsSet("Charm") && npcCaster.Master == Victim)
            return "You can't do that on your own follower.";
        // victim found
        return null;
    }

    public IEnumerable<IEntity> ValidTargets(ICharacter caster) // is intended to be used by inherited class overriding SetTargets
        => caster.Room.People.Where(x => x != caster && caster.CanSee(x) && IsTargetValid(caster, x));

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
