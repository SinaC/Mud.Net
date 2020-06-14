using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class CauseLight : DamageSpellBase
    {
        public const string SpellName = "Cause Light";

        public CauseLight(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Harm;
        protected override int DamageValue => RandomManager.Dice(1, 8) + Level / 3;
        protected override string DamageNoun => "spell";
    }
}
