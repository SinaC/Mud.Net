using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
[Help(
@"Shield block is a rather fancy name for the art of parrying with a shield.
Characters with no shield block skill will not be able to defend themselves
well with a shield.  All classes may learn shield block, but only warriors and
clerics are good at it.  Beware, flails ignore shield blocking attempts, and
whips have an easier time getting around them.  Axes may split shields in two.")]
[OneLineHelp("the art of parrying with a shield")]
public class ShieldBlock : HitAvoidancePassiveBase
{
    private const string PassiveName = "Shield Block";

    public ShieldBlock(ILogger<ShieldBlock> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string AvoiderPhrase => "You block {0}'s attack with your shield.";
    protected override string AggressorPhrase => "{0:N} blocks your attack with a shield.";

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        if (user.Position <= Positions.Sleeping || user.Stunned > 0)
            return false;

        if (user.GetEquipment<IItemShield>(EquipmentSlots.OffHand) == null)
            return false;

        int chance = 3 + learnPercentage / 5;
        chance += user.Level - victim.Level;

        return diceRoll < chance;
    }
}
