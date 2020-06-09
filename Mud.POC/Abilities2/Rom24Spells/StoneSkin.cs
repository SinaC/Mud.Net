using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18)]
    [AbilityCharacterWearOffMessage("Your skin feels soft again.")]
    [AbilityDispellable("{0:N}'s skin regains its normal texture.")]
    public class StoneSkin : DefensiveSpellBase
    {
        public const string SpellName = "Stone Skin";

        private IAuraManager AuraManager { get; }

        public StoneSkin(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(SpellName) != null)
            {
                if (Victim == Caster)
                    Caster.Send("Your skin is already as hard as a rock.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} is already as hard as can be.", Victim);
                return;
            }

            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -40, Operator = AffectOperators.Add });
            Caster.Act(ActOptions.ToAll, "{0:P} skin turns to stone.", Victim);
        }
    }
}
