using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class NoTargetSpellBase : SpellBase
    {
        protected NoTargetSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(SpellActionInput spellActionInput) => null;
    }
}
