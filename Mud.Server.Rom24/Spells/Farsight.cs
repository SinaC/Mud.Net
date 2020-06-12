using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
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
            Caster.Send("Not Yet Implemented."); // TODO: affect giving longer range when using scan command
        }
    }
}
