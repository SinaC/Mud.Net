using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class Parry : HitAvoidancePassiveBase
{
    private const string PassiveName = "Parry";

    public Parry(IRandomManager randomManager)
        : base(randomManager)
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
