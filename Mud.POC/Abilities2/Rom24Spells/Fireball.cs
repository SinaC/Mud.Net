using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Fireball", AbilityEffects.Damage)]
    public class Fireball : DamageTableSpellBase
    {
        public Fireball(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int[] Table =>
            new []
            {
                0,
                0,   0,   0,   0,   0,      0,   0,   0,   0,   0,
                0,   0,   0,   0,  30,     35,  40,  45,  50,  55,
                60,  65,  70,  75,  80,     82,  84,  86,  88,  90,
                92,  94,  96,  98, 100,    102, 104, 106, 108, 110,
                112, 114, 116, 118, 120,    122, 124, 126, 128, 130
            };

        protected override SchoolTypes DamageType => SchoolTypes.Fire;
        protected override string DamageNoun => "fireball";
    }
}
