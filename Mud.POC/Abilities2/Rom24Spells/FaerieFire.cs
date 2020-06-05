using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Faerie Fire", AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("The pink aura around you fades away.")]
    [AbilityDispellable("{0:N}'s outline fades.")]
    public class FaerieFire : OffensiveSpellBase
    {
        private IAuraManager AuraManager { get; }

        public FaerieFire(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.FaerieFire))
                return;
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 2 * Level, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.FaerieFire, Operator = AffectOperators.Or });
            Victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", Victim);
        }
    }
}
