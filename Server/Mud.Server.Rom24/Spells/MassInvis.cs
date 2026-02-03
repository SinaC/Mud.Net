using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
[AbilityCharacterWearOffMessage("You are no longer invisible.")]
[AbilityDispellable("{0:N} fades into existence.")]
[Syntax("cast [spell]")]
[Help(
@"The MASS INVIS spell makes all characters in the caster's group invisible,
including the caster.")]
[OneLineHelp("turns the caster's group invisible")]
public class MassInvis : NoTargetSpellBase
{
    private const string SpellName = "Mass Invis";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private IAuraManager AuraManager { get; }

    public MassInvis(ILogger<MassInvis> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        foreach (var victim in Caster.Room.People.Where(Caster.CanSee))
        {
            if (Caster.IsSameGroupOrPet(victim))
            {
                victim.Act(ActOptions.ToAll, "{0:N} slowly fade{0:v} out of existence.", victim);

                AuraManager.AddAura(victim, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(24), new AuraFlags(), true,
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Invisible"), Operator = AffectOperators.Or });
            }
        }
        Caster.Send("Ok.");
    }
}
