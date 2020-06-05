using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySpellBase<TItem> : SpellBase
        where TItem : class, IItem
    {
        protected TItem Item { get; private set; }

        protected ItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            if (abilityActionInput.Parameters.Length < 1)
                return "What should the spell be cast upon?";
            IItem target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), abilityActionInput.Parameters[0]); // TODO: equipments ?
            if (target == null)
                return "You are not carrying that.";
            if (!(target is TItem))
                return InvalidItemTypeMsg;
            Item = target as TItem;
            return null;
        }

        protected abstract string InvalidItemTypeMsg { get; }
    }
}
