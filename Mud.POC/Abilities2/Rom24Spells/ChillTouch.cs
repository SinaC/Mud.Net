using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ChillTouch : CharacterDamageTableSpellBase, IAbilityCharacterBuff, IAbilityDispellable
    {
        public override int Id => 15;
        public override string Name => "Chill Touch";

        private IAuraManager AuraManager { get; }
        public ChillTouch(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public string CharacterWearOffMessage => "You feel less cold.";
        public string DispelRoomMessage => "{0:N} looks warmer.";

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

        protected override void PostDamage(ICharacter caster, int level, ICharacter victim, bool savesSpellResult, DamageResults damageResult)
        {
            if (!savesSpellResult)
            {
                victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", victim);
                IAura existingAura = victim.GetAura(this);
                if (existingAura != null)
                {
                    existingAura.Update(level, TimeSpan.FromHours(6));
                    existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.Strength,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        x => x.Modifier -= 1);
                }
                else
                    AuraManager.AddAura(victim, this, caster, level, TimeSpan.FromHours(6), AuraFlags.None, true,
                        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
            }
        }
    }
}
