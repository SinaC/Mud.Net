using Mud.POC.Abilities2.Domain;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
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
