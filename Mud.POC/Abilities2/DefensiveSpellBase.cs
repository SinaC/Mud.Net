using Mud.Server.Common;
using Mud.Server.Input;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Helpers;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public abstract class DefensiveSpellBase : SpellBase
    {
        protected ICharacter Victim { get; private set; }

        public DefensiveSpellBase(IRandomManager randomManager, IWiznet wiznet) 
            : base(randomManager, wiznet)
        {
        }

        #region SpellBase

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (Victim == null)
                return;
            Action(caster, level, Victim);
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter target;
            if (parameters.Length < 1)
                target = caster;
            else
            {
                target = FindHelpers.FindByName(caster.Room.People, parameters[0]);
                if (target == null)
                {
                    caster.Send("They aren't here.");
                    return AbilityTargetResults.TargetNotFound;
                }
            }
            // victim found
            Victim = target;
            return AbilityTargetResults.Ok;
        }

        #endregion

        protected abstract void Action(ICharacter caster, int level, ICharacter victim);
    }
}
