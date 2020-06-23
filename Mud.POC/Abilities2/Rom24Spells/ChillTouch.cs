using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You feel less cold.")]
    [AbilityDispellable("{0:N} looks warmer.")]
    public class ChillTouch : DamageTableSpellBase
    {
        public const string SpellName = "Chill Touch";

        private IAuraManager AuraManager { get; }

        public ChillTouch(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override SchoolTypes DamageType => SchoolTypes.Cold;
        protected override string DamageNoun => "chill touch";
        protected override int[] Table => new [] 
        {
             0,
             0,  0,  6,  7,  8,  9, 12, 13, 13, 13,
            14, 14, 14, 15, 15, 15, 16, 16, 16, 17,
            17, 17, 18, 18, 18, 19, 19, 19, 20, 20,
            20, 21, 21, 21, 22, 22, 22, 23, 23, 23,
            24, 24, 24, 25, 25, 25, 26, 26, 26, 27
        };

        protected override void Invoke()
        {
            base.Invoke();
            if (SavesSpellResult || DamageResult != DamageResults.Done)
                return;
            Victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", Victim);
            IAura existingAura = Victim.GetAura(SpellName);
            if (existingAura != null)
            {
                existingAura.Update(Level, TimeSpan.FromMinutes(6));
                existingAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.Strength,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                    x => x.Modifier -= 1);
            }
            else
                AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(6), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
        }
    }
}
