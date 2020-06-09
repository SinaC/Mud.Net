using Mud.POC.Abilities2.Domain;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class CauseSerious : DamageSpellBase
    {
        public const string SpellName = "Cause Serious";

        public CauseSerious(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Harm;
        protected override int DamageValue => RandomManager.Dice(2, 8) + Level / 2;
        protected override string DamageNoun => "spell";
    }
}
