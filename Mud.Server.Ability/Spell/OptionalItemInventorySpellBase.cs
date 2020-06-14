using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Linq;

namespace Mud.Server.Ability.Spell
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; set; }

        protected OptionalItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
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
