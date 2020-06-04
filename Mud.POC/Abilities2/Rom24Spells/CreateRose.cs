using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Create Rose", AbilityEffects.Creation)]
    public class CreateRose : ItemCreationSpellBase
    {
        public CreateRose(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings) 
            : base(randomManager, wiznet, itemManager, settings)
        {
        }

        protected override void Invoke()
        {
            //TODO: add rose blueprint
        }

        public override string Guards(AbilityActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;
            return "Not Yet Implemented";
        }
    }
}
