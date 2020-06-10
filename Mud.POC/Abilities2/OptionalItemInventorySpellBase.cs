using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; set; }

        protected OptionalItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Item = spellActionInput.CastFromItemOptions.PredefinedTarget as IItem;
                return null;
            }

            Item = null;
            if (spellActionInput.Parameters.Length >= 1)
            {
                Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
                if (Item == null)
                    return "You are not carrying that.";
            }
            return null;
        }
    }
}
