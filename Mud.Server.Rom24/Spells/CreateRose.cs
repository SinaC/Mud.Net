using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings;

namespace Mud.Server.Rom24.Spells
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
