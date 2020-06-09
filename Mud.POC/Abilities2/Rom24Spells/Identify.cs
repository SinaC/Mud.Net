using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
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
