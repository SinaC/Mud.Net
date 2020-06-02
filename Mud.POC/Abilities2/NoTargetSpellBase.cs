using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class NoTargetSpellBase : SpellBase
    {
        protected NoTargetSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke(ICharacter caster, int level, IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            Action(caster, level, rawParameters, parameters);
        }

        protected override AbilityTargetResults GetTarget(ICharacter caster, out IEntity target, string rawParameters, params CommandParameter[] parameters)
        {
            target = null;
            return AbilityTargetResults.Ok;
        }

        public abstract void Action(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters);
    }
}
