using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
[Help(
@"The fast healing skill improves wound healing rates, whether walking, resting,
or sleeping. It represents knowledge of healing herbs or just general 
toughness and stamina.  Fast healing is checked every tick, and it is 
possible for it to fail.  All class may learn this skill, but mages find it
very difficult to master, due to their bookish lifestyle.")]
public class FastHealing : RegenerationPassiveBase
{
    private const string PassiveName = "Fast Healing";

    public FastHealing(ILogger<FastHealing> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override int HitGainModifier(ICharacter user, int baseHitGain)
    {
        bool isTriggered = IsTriggered(user, user, false, out _, out int learnPercentage);
        if (isTriggered)
        {
            if (user.HitPoints < user.MaxHitPoints)
                (user as IPlayableCharacter)?.CheckAbilityImprove(PassiveName, true, 8);
            return (learnPercentage * baseHitGain) / 100;
        }
        return 0;
    }
}
