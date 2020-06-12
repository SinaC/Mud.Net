using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Detection)]
    public class Identify : ItemInventorySpellBase
    {
        public const string SpellName = "Identify";

        public Identify(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            // TODO
            Caster.Send("Not Yet Implemented.");
        }
    }
}
