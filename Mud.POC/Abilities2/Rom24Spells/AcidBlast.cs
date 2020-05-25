using Mud.Server.Common;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class AcidBlast : CharacterDamageSpellBase
    {
        public override int Id => 1;
        public override string Name => "Acid Blast";

        public AcidBlast(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Acid;
        protected override int DamageValue(int level) => RandomManager.Dice(level, 12);
        protected override string DamageNoun => "acid blast";
    }
}
