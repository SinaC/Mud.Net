using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 8)]
public class Meditation : RegenerationPassiveBase
{
    private const string PassiveName = "Meditation";

    public Meditation(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    public override int ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, int baseResourceGain)
    {
        if (resourceKind == ResourceKinds.Mana)
        {
            bool isTriggered = IsTriggered(user, user, false, out _, out int learnPercentage);
            if (isTriggered)
            {
                if (user[ResourceKinds.Mana] < user.MaxResource(ResourceKinds.Mana))
                    (user as IPlayableCharacter)?.CheckAbilityImprove(PassiveName, true, 8);
                return (learnPercentage * baseResourceGain) / 100;
            }
        }
        return 0;
    }
}
