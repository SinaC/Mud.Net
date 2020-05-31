using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySpellBase<TItem> : SpellBase
        where TItem : IItem
    {
        protected TItem Item { get; private set; }

        protected ItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (Item == null)
                return;
            Action(caster, level, Item);
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 1)
            {
                caster.Send("What should the spell be cast upon?");
                return AbilityTargetResults.MissingParameter;
            }
            IItem target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
            if (target == null)
            {
                caster.Send("You are not carrying that.");
                return AbilityTargetResults.TargetNotFound;
            }
            if (!(target is TItem))
            {
                caster.Send(InvalidItemTypeMsg);
                return AbilityTargetResults.InvalidTarget;
            }
            Item = (TItem)target;
            return AbilityTargetResults.Ok;
        }

        #endregion

        protected abstract string InvalidItemTypeMsg { get; }

        public abstract void Action(ICharacter caster, int level, TItem item);
    }
}
