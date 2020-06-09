using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

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

        public override string Setup(AbilityActionInput actionInput)
        {
            string baseGuards = base.Setup(actionInput);
            if (baseGuards != null)
                return baseGuards;
            return "Not Yet Implemented";
        }
    }
}
