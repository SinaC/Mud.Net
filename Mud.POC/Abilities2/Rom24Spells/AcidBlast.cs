using Mud.Server.Common;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Acid Blast", AbilityEffects.Damage)]
    public class AcidBlast : DamageSpellBase
    {
        public AcidBlast(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Acid;
        protected override int DamageValue => RandomManager.Dice(Level, 12);
        protected override string DamageNoun => "acid blast";
    }
}
