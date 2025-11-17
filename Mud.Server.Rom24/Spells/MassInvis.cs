using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
[AbilityCharacterWearOffMessage("You are no longer invisible.")]
[AbilityDispellable("{0:N} fades into existence.")]
public class MassInvis : NoTargetSpellBase
{
    private const string SpellName = "Mass Invis";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public MassInvis(IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(randomManager)
    {
        ServiceProvider = serviceProvider;
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
                    new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Invisible"), Operator = AffectOperators.Or });
            }
        }
        Caster.Send("Ok.");
    }
}
