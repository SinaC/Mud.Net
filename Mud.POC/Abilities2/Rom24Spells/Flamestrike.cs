using Mud.POC.Abilities2.Domain;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class Flamestrike : DamageSpellBase
    {
        public const string SpellName = "Flamestrike";

        public Flamestrike(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Fire;
        protected override int DamageValue => RandomManager.Dice(6 + Level / 2, 8);
        protected override string DamageNoun => "flamestrike";
    }
}
