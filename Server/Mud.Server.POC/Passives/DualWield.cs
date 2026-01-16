using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.POC.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
public class DualWield : PassiveBase, IAdditionalHitPassive
{
    private const string PassiveName = "Dual Wield";

    protected override string Name => PassiveName;

    public DualWield(ILogger<DualWield> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public int AdditionalHitIndex => 2;
    public bool StopMultiHitIfFailed => false; // continue multi hit even if dual wield failed

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var offHand = user.GetEquipment<IItemWeapon>(EquipmentSlots.OffHand);

        if (offHand == null)
            return false;

        var chance = 2 * learnPercentage / 3 + 33;
        return diceRoll < chance;
    }
}
