using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ColourSpray : CharacterDamageTableSpellBase
    {
        public override int Id => 16;
        public override string Name => "Colour Spray";

        private IAuraManager AuraManager { get; }
        public ColourSpray(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override SchoolTypes DamageType => SchoolTypes.Light;
        protected override string DamageNoun => "colour spray";
        protected override int[] Table => new[]
        {
             0,
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
            58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
            65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
            73, 73, 74, 75, 76, 76, 77, 78, 79, 79
        };

        protected override void PostDamage(ICharacter caster, int level, ICharacter victim, bool savesSpellResult, DamageResults damageResult)
        {
            if (!savesSpellResult && damageResult == DamageResults.Done)
            {
                Blindness blindness = new Blindness(RandomManager, Wiznet, AuraManager);
                blindness.Action(caster, level/2, victim);
            }
        }
    }
}
