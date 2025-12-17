using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"Dispel good brings forth evil energies that inflict horrific torment on 
the pure of heart.  Good-aligned characters use this dark magic at their
peril.")]
[OneLineHelp("calls down unholy power on good creatures")]
public class DispelGood : DamageSpellBase
{
    private const string SpellName = "Dispel Good";

    public DispelGood(ILogger<DispelGood> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Negative;
    protected override string DamageNoun => "dispel good";
    protected override int DamageValue
        => Victim.CurrentHitPoints >= Caster.Level * 4
            ? RandomManager.Dice(Level, 4)
            : Math.Max(Victim.CurrentHitPoints, RandomManager.Dice(Level, 4));

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        var baseSetTargets = base.SetTargets(spellActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        // Check alignment
        if (Caster is IPlayableCharacter && Caster.IsGood)
            Victim = Caster;

        if (Victim.IsEvil)
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
