using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.MegaSpells
{
    [Spell("General Purpose", AbilityEffects.Damage)]
    public class GeneralPurpose : DamageSpellBase
    {
        public GeneralPurpose(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Pierce;
        protected override int DamageValue => RandomManager.Range(25, 100);
        protected override string DamageNoun => "general purpose ammo";
    }
}
