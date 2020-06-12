using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.MegaSpells
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
