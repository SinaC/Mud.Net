using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
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
