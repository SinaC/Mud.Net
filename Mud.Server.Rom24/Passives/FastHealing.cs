using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
public class FastHealing : RegenerationPassiveBase
{
    private const string PassiveName = "Fast Healing";

    public FastHealing(IRandomManager randomManager)
        : base(randomManager)
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
