using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class SpellBurningHands : CharacterDamageTableSpellBase
    {
        public override int Id => 5;
        public override string Name => "Burning Hands";

        public SpellBurningHands(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }
        protected override SchoolTypes DamageType => SchoolTypes.Fire;
        protected override string DamageNoun => "burning hands";
        protected override int[] Table => new[]
        {
             0,
             0,  0,  0,  0, 14, 17, 20, 23, 26, 29,
            29, 29, 30, 30, 31, 31, 32, 32, 33, 33,
            34, 34, 35, 35, 36, 36, 37, 37, 38, 38,
            39, 39, 40, 40, 41, 41, 42, 42, 43, 43,
            44, 44, 45, 45, 46, 46, 47, 47, 48, 48
        };

    }
}
