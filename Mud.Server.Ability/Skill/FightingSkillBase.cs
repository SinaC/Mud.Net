using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Skill
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
