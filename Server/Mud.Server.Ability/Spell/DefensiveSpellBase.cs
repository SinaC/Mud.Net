using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Random;

namespace Mud.Server.Ability.Spell;

public abstract class DefensiveSpellBase : SpellBase, ITargetedAction
{
    protected DefensiveSpellBase(ILogger<DefensiveSpellBase> logger, IRandomManager randomManager) 
        : base(logger, randomManager)
    {
    }
    protected ICharacter Victim { get; set; } = default!;

    public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Room.People.Where(caster.CanSee);

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter ?? Caster;
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
            Victim = Caster;
        else
        {
            Victim = FindHelpers.FindByName(ValidTargets(Caster).OfType<ICharacter>(), spellActionInput.Parameters[0])!;
            if (Victim == null)
                return StringHelpers.CharacterNotFound;
        }
        // victim found
        return null;
    }
}
