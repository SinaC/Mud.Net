using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class CauseCritical : DamageSpellBase
    {
        public const string SpellName = "Cause Critical";

        public CauseCritical(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Harm;
        protected override int DamageValue => RandomManager.Dice(3, 8) + Level - 6;
        protected override string DamageNoun => "spell";
    }
}
