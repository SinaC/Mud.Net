using Mud.POC.Abilities2.Domain;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24MegaSpells
{
    [Spell("High Explosive", AbilityEffects.Damage)]
    public class HighExplosive : DamageSpellBase
    {
        public HighExplosive(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Pierce;
        protected override int DamageValue => RandomManager.Range(30, 120);
        protected override string DamageNoun => "high explosive ammo";
    }
}
