using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected OptionalItemInventorySpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            Action(caster, level, target as IItem);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            IItem item = null;
            target = null;
            if (parameters.Length >= 1)
            {
                item = FindHelpers.FindByName(caster.Inventory, parameters[0]); // TODO: equipments ?
                if (item == null)
                {
                    caster.Send("You are not carrying that.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            target = item;
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, IItem item);
    }
}
