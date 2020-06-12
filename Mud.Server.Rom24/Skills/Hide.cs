using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills
{
    [Command("hide", "Abilities", "Skills")]
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

            if (User.CharacterFlags.HasFlag(CharacterFlags.Hide))
                User.RemoveBaseCharacterFlags(CharacterFlags.Hide);

            bool success = false;
            if (RandomManager.Chance(Learned))
            {
                User.AddBaseCharacterFlags(CharacterFlags.Hide);
                success = true;
            }

            User.Recompute();
            return success;
        }
    }
}
