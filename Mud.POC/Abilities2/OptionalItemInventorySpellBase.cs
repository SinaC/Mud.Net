using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; private set; }

        protected OptionalItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            Item = null;
            if (abilityActionInput.Parameters.Length >= 1)
            {
                Item = FindHelpers.FindByName(Caster.Inventory, abilityActionInput.Parameters[0]); // TODO: equipments ?
                if (Item == null)
                    return "You are not carrying that.";
            }
            return null;
        }
    }
}
