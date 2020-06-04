using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class NoTargetSpellBase : SpellBase
    {
        protected NoTargetSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput) => null;
    }
}
