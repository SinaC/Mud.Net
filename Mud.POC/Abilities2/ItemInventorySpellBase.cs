using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; set; }

        protected ItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => caster.Inventory.Where(caster.CanSee);

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            if (spellActionInput.Parameters.Length < 1)
                return "What should it be used upon?";
            Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
            if (Item == null)
                return "You are not carrying that.";
            return null;
        }
    }

    public abstract class ItemInventorySpellBase<TItem> : SpellBase
        where TItem : class, IItem
    {
        protected TItem Item { get; set; }

        protected ItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => caster.Inventory.OfType<TItem>().Where(caster.CanSee);

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
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
