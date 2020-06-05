using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Frenzy", AbilityEffects.Buff, PulseWaitTime = 24)]
    [AbilityCharacterWearOffMessage("Your rage ebbs.")]
    [AbilityDispellable("{0:N} no longer looks so wild.")]
    public class Frenzy : DefensiveSpellBase
    {
        private IAuraManager AuraManager { get; }

        public Frenzy(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Berserk) || Victim.GetAura("Frenzy") != null)
            {
                if (Victim == Caster)
                    Caster.Send("You are already in a frenzy.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} is already in a frenzy.", Victim);
                return;
            }

            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Calm) || Victim.GetAura("Calm") != null)
            {
                if (Victim == Caster)
                    Caster.Send("Why don't you just relax for a while?");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", Victim);
                return;
            }

            if ((Caster.IsGood && !Victim.IsGood)
                || (Caster.IsNeutral && !Victim.IsNeutral)
                || (Caster.IsEvil && !Victim.IsEvil))
            {
                Caster.Act(ActOptions.ToCharacter, "Your god doesn't seem to like {0:N}.", Victim);
                return;
            }

            int duration = Level / 3;
            int modifier = Level / 6;
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = (10 * Level) / 12, Operator = AffectOperators.Add });

            Victim.Send("You are filled with holy wrath!");
            Victim.Act(ActOptions.ToRoom, "{0:N} gets a wild look in $s eyes!", Victim);
        }
    }
}
