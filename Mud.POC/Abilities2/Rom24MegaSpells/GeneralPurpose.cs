using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24MegaSpells
{
    [Spell("General Purpose", AbilityEffects.Damage)]
    public class GeneralPurpose : DamageSpellBase
    {
        public GeneralPurpose(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Pierce;
        protected override int DamageValue => RandomManager.Range(25, 100);
        protected override string DamageNoun => "general purpose ammo";
    }
}
