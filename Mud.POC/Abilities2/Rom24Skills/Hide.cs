using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
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
