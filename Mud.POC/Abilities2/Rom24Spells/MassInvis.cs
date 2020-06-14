using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
    [AbilityCharacterWearOffMessage("You are no longer invisible.")]
    [AbilityDispellable("{0:N} fades into existence.")]
    public class MassInvis : NoTargetSpellBase
    {
        public const string SpellName = "Mass Invis";

        private IAuraManager AuraManager { get; }

        public MassInvis(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People.Where(Caster.CanSee))
            {
                if (Caster.IsSameGroupOrPet(victim))
                {
                    victim.Act(ActOptions.ToAll, "{0:N} slowly fade{0:v} out of existence.", victim);

                    AuraManager.AddAura(victim, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
                }
            }
            Caster.Send("Ok.");
        }
    }
}
