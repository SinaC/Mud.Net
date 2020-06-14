using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
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
