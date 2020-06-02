using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrDefensiveSpellBase : SpellBase
    {
        protected ItemOrDefensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (target == null)
                return;
            if (target is ICharacter victim)
                Action(caster, level, victim);
            else if (target is IItem item)
                Action(caster, level, item);
            else
                Wiznet.Wiznet($"{GetType().Name}: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = parameters.Length < 1
                        ? caster
                        : FindHelpers.FindByName(caster.Room.People, parameters[0]);
            if (target == null)
            {
                target = FindHelpers.FindByName(caster.Inventory, parameters[0]);
                if (target == null)
                {
                    caster.Send("You don't see that here.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            // victim or item (target) found
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, ICharacter victim);
        public abstract void Action(ICharacter caster, int level, IItem item);
    }
}
