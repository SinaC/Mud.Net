using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySpellBase : SpellBase, ITargetedAction
    {
        protected IItem Item { get; set; }

        protected ItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Inventory.Where(caster.CanSee);

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Item = spellActionInput.CastFromItemOptions.PredefinedTarget as IItem;
                if (Item == null)
                    return "You can't do that.";
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
                return "What should it be used upon?";
            Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
            if (Item == null)
                return "You are not carrying that.";
            return null;
        }
    }

    public abstract class ItemInventorySpellBase<TItem> : SpellBase, ITargetedAction
        where TItem : class, IItem
    {
        protected TItem Item { get; set; }

        protected ItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Inventory.OfType<TItem>().Where(caster.CanSee);

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Item = spellActionInput.CastFromItemOptions.PredefinedTarget as TItem;
                if (Item == null)
                    return "You can't do that.";
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
                return IsCastFromItem
                    ? "What should it be used upon?"
                    : "What should the spell be cast upon?";
            IItem target = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
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
