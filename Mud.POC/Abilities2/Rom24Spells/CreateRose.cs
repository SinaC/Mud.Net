using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Creation)]
    public class CreateRose : ItemCreationSpellBase
    {
        public const string SpellName = "Create Rose";

        public CreateRose(IRandomManager randomManager, IItemManager itemManager, ISettings settings) 
            : base(randomManager, itemManager, settings)
        {
        }

        protected override void Invoke()
        {
            //TODO: add rose blueprint
        }

        public override string Setup(ISpellActionInput spellActionInput)
        {
            string baseSetup = base.Setup(spellActionInput);
            if (baseSetup != null)
                return baseSetup;

            return "Not Yet Implemented";
        }
    }
}
