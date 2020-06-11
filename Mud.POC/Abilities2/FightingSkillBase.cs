using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class FightingSkillBase : SkillBase
    {
        protected ICharacter Victim { get; set; }

        protected FightingSkillBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            Victim = User.Fighting;
            if (Victim == null)
                return "You aren't fighting anyone.";
            // Victim found
            return null;
        }
    }
}
