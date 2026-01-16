using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
[Help(
@"This skill is similar to fast healing, but relies on the concentration and
mantras to increase mana recovery when the character is sleeping or resting.
Thieves and warriors, with their troubled minds and violent attitudes, have
much trouble learning to meditate.
Meditation level 2 is an advanced form of the meditation skill.  Characters
skilled in meditation 2 will recover mana at an even faster rate than those
only skilled in meditation. Meditation works automatically.  It does not 
require any command word.")]
public class Meditation : RegenerationPassiveBase
{
    private const string PassiveName = "Meditation";

    protected override string Name => PassiveName;

    public Meditation(ILogger<Meditation> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override decimal ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, decimal baseResourceGain)
    {
        if (resourceKind == ResourceKinds.Mana)
        {
            bool isTriggered = IsTriggered(user, user, false, out int diceRoll, out _);
            if (isTriggered)
            {
                if (user[ResourceKinds.Mana] < user.MaxResource(ResourceKinds.Mana))
                    (user as IPlayableCharacter)?.CheckAbilityImprove(PassiveName, true, 8);
                return (diceRoll * baseResourceGain) / 100m;
            }
        }
        return 0;
    }
}
