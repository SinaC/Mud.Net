using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySpellBase<TItem> : SpellBase
        where TItem : class, IItem
    {
        protected ItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (target == null)
                return;
            Action(caster, level, target as TItem);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = null;
            if (parameters.Length < 1)
            {
                caster.Send("What should the spell be cast upon?");
                return AbilityTargetResults.MissingParameter;
            }
            target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
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
            return AbilityTargetResults.Ok;
        }

        #endregion

        protected abstract string InvalidItemTypeMsg { get; }

        public abstract void Action(ICharacter caster, int level, TItem item);
    }
}
