using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.None)]
    public class Farsight : NoTargetSpellBase
    {
        public const string SpellName = "Farsight";

        public Farsight(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            Caster.Send("Not Yet Implemented.");
        }
    }
}
