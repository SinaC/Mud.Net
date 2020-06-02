using Mud.Server.Common;
using Mud.Server.Input;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Helpers;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public abstract class DefensiveSpellBase : SpellBase
    {
        protected DefensiveSpellBase(IRandomManager randomManager, IWiznet wiznet) 
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            if (target == null)
                return;
            Action(caster, level, target as ICharacter);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter victim;
            target = null;
            if (parameters.Length < 1)
                victim = caster;
            else
            {
                victim = FindHelpers.FindByName(caster.Room.People, parameters[0]);
                if (victim == null)
                {
                    caster.Send("They aren't here.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            // victim found
            target = victim;
            return AbilityTargetResults.Ok;
        }

        #endregion

        public abstract void Action(ICharacter caster, int level, ICharacter victim);
    }
}
