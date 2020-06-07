using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24MegaSpells
{
    [Spell("High Explosive", AbilityEffects.Damage)]
    public class HighExplosive : DamageSpellBase
    {
        public HighExplosive(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Pierce;
        protected override int DamageValue => RandomManager.Range(30, 120);
        protected override string DamageNoun => "high explosive ammo";
    }
}
