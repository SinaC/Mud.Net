using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class ItemOrDefensiveSpellBase : SpellBase
    {
        protected IEntity Target { get; private set; }

        protected ItemOrDefensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (Target == null)
                return;
            Action(caster, level, Target);
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            IEntity target = parameters.Length < 1
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
            Target = target;
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, IEntity target);
    }
}
