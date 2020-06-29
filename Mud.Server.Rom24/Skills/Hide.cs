using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills
{
    [CharacterCommand("hide", "Ability", "Skill")]
    [Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
    public class Hide : NoTargetSkillBase
    {
        public const string SkillName = "Hide";

        public Hide(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override bool Invoke()
        {
            User.Send("You attempt to hide.");

            if (User.CharacterFlags.IsSet("Hide"))
                User.RemoveBaseCharacterFlags(false, "Hide");

            bool success = false;
            if (RandomManager.Chance(Learned))
            {
                User.AddBaseCharacterFlags(false, "Hide");
                success = true;
            }

            User.Recompute();
            return success;
        }
    }
}
