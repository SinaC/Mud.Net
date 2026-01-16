using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
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

                AuraManager.AddAura(victim, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Invisible"), Operator = AffectOperators.Or });
            }
        }
        Caster.Send("Ok.");
    }
}
