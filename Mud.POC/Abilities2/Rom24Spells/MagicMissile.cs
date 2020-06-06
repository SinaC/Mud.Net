using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Magic Missile", AbilityEffects.Damage)]
    public class MagicMissile : DamageTableSpellBase
    {
        public MagicMissile(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int[] Table => new int []
        {
             0,
             3,  3,  4,  4,  5,  6,  6,  6,  6,  6,
             7,  7,  7,  7,  7,  8,  8,  8,  8,  8,
             9,  9,  9,  9,  9, 10, 10, 10, 10, 10,
            11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
            13, 13, 13, 13, 13, 14, 14, 14, 14, 14
        };
        protected override SchoolTypes DamageType => SchoolTypes.Energy;
        protected override string DamageNoun => "magic missile";
    }
}
