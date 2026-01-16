using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
[Help(
@"If at first you fail to dodge, block it.  Parry is useful for deflecting 
attacks, and is succesful more often than dodge.  Parry requires a weapon for
full success, the hand-to-hand skill may also be used, but results in reduced
damage instead of no damage.  The best chance of parrying occurs when the
defender is skilled in both his and his opponent's weapon type.")]
[OneLineHelp("the art of parrying with weapons")]
public class Parry : HitAvoidancePassiveBase
{
    private const string PassiveName = "Parry";

    protected override string Name => PassiveName;

    public Parry(ILogger<Parry> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string AvoiderPhrase => "You parry {0}'s attack.";
    protected override string AggressorPhrase => "{0:N} parries your attack.";

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        if (user.Position <= Positions.Sleeping || user.Stunned > 0)
            return false;

        int chance = learnPercentage / 2;

        if (user.GetEquipment(EquipmentSlots.MainHand) == null)
        {
            if (user is IPlayableCharacter) // player must have a weapon to parry
                return false;
            chance /= 2;
        }

        if (!user.CanSee(victim))
            chance /= 2;

        chance += user.Level - victim.Level;

        return diceRoll < chance;
    }
}
