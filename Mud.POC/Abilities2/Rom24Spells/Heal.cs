using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Heal", AbilityEffects.Healing)]
    public class Heal : HealSpellBase
    {
        public Heal(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override int HealValue => 100;

        protected override string HealVictimPhrase => "A warm feeling fills your body.";
    }
}
