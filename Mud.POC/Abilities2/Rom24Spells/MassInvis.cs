using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Mass Invis", AbilityEffects.Buff, PulseWaitTime = 24)]
    [AbilityCharacterWearOffMessage("You are no longer invisible.")]
    [AbilityDispellable("{0:N} fades into existance.")]
    public class MassInvis : NoTargetSpellBase
    {
        private IAuraManager AuraManager { get; }

        public MassInvis(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People)
            {
                if (Caster.IsSameGroupOrPet(victim))
                {
                    victim.Act(ActOptions.ToAll, "{0:N} slowly fade{0:v} out of existence.", victim);

                    AuraManager.AddAura(victim, AbilityInfo.Name, Caster, Level / 2, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
                }
            }
            Caster.Send("Ok.");
        }
    }
}
