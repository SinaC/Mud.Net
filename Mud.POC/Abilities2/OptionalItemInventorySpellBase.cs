using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; private set; }

        protected OptionalItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
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
            IItem target = null;
            if (parameters.Length >= 1)
            {
                target = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
                if (target == null)
                {
                    caster.Send("You are not carrying that.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            Item = target;
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, IItem item);
    }
}
