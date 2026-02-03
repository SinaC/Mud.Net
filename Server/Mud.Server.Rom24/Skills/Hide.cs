using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Skill;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("hide", "Ability", "Skill")]
[Skill(SkillName, AbilityEffects.Buff, LearnDifficultyMultiplier = 3)]
[Help(
@"Hide and sneak are similar skills, both related to remaining undetected.
Hide has a very high chance of success, but only works for as long as the
character remains stationary.  Sneak may be used when moving (including to
sneak by monsters), but has a lower chance of success.  Typing hide or sneak
a second time will cancel them.  Hide has the added benefit of increasing
the chance of a backstab hitting your opponent.")]
[OneLineHelp("the art of remaining undetected in a room")]
public class Hide : NoTargetSkillBase
{
    private const string SkillName = "Hide";

    public Hide(ILogger<Hide> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override bool Invoke()
    {
        User.Send("%W%You attempt to hide.%x%");

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
