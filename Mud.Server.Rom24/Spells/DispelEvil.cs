using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class DispelEvil : DamageSpellBase
{
    private const string SpellName = "Dispel Evil";

    public DispelEvil(ILogger<DispelEvil> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Holy;
    protected override string DamageNoun => "dispel evil";
    protected override int DamageValue
        => Victim.HitPoints >= Caster.Level * 4
            ? RandomManager.Dice(Level, 4)
            : Math.Max(Victim.HitPoints, RandomManager.Dice(Level, 4));

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        var baseSetTargets = base.SetTargets(spellActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        // Check alignment
        if (Caster is IPlayableCharacter && Caster.IsEvil)
            Victim = Caster;

        if (Victim.IsGood)
        {
            Caster.Act(ActOptions.ToAll, "Mota protects {0:N}.", Victim);
            return string.Empty; // TODO: should return above message
        }
        if (Victim.IsNeutral)
        {
            Caster.Act(ActOptions.ToCharacter, "{0:N} does not seem to be affected.", Victim);
            return string.Empty; // TODO: should return above message
        }

        return null;
    }
}
