using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
[Help(
@"In the words of one wise warrior, 'the best way to block a blow is to not
be where it lands'.  The dodge skill honors this tradition, by improving the
character's natural agility to the point where many blows will miss the 
target. The chance of dodging is also affected by the dexterity of the
attacker and the target.  Any class may learn dodging.")]
[OneLineHelp("the best way to take a punch is not to be there")]
public class Dodge : HitAvoidancePassiveBase
{
    private const string PassiveName = "Dodge";

    protected override string Name => PassiveName;

    public Dodge(ILogger<Dodge> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string AvoiderPhrase => "You dodge {0}'s attack.";

    protected override string AggressorPhrase => "{0:N} dodges your attack.";

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        if (user.Position <= Positions.Sleeping || user.Stunned > 0)
            return false;

        int chance = learnPercentage / 2;
        
        if (!user.CanSee(victim))
            chance /= 2;

        chance += user.Level - victim.Level;

        return diceRoll < chance;
    }
}
