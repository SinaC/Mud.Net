using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("rescue", "Ability", "Skill", "Combat")]
[Syntax("[cmd] <victim>")]
[Skill(SkillName, AbilityEffects.None)]
public class Rescue : OffensiveSkillBase
{
    private const string SkillName = "Rescue";

    public Rescue(ILogger<Rescue> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        var baseSetTargets = base.SetTargets(skillActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        if (User == Victim)
            return "What about fleeing instead?";

        if (Victim is INonPlayableCharacter && User is IPlayableCharacter)
            return "Doesn't need your help!";

        if (User.Fighting == Victim)
            return "Too late.";

        var fighting = Victim.Fighting;
        if (fighting == null)
            return "That person is not fighting right now.";

        if (fighting is INonPlayableCharacter && User.IsSameGroupOrPet(Victim))
            return "Kill stealing is not permitted.";

        return null;
    }

    protected override bool Invoke()
    {
        if (!RandomManager.Chance(Learned))
        {
            User.Send("You fail the rescue.");
            return false;
        }

        User.Act(ActOptions.ToAll, "{0:N} rescue{0:v} {1:N}.", User, Victim);
        Victim.Fighting!.StopFighting(false);
        Victim.StopFighting(false);
        User.StopFighting(false);

        // TODO: check killer
        User.StartFighting(Victim);
        Victim.StartFighting(User);

        return true;
    }
}
