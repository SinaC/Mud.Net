using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.POC.Passives;

public abstract class AdditionalWieldPassiveBase: PassiveBase, IAdditionalWieldPassive
{
    protected AdditionalWieldPassiveBase(ILogger<AdditionalWieldPassiveBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected abstract int WieldCount { get; }

    public abstract int AdditionalHitIndex { get; }
    public abstract bool StopMultiHitIfFailed { get; }

    public bool IsTriggered(ICharacter user, ICharacter victim, bool checkImprove, out int diceRoll, out int learnPercentage, out IItemWeapon? weapon)
    {
        weapon = user.Equipments
            .Where(x => x.Item != null && x.Item is IItemWeapon)
            .OrderBy(x => ConvertSlotToOrderIndex(x.Slot))
            .Select(x => x.Item)
            .OfType<IItemWeapon>()
            .ElementAtOrDefault(WieldCount-1);

        if (weapon == null)
        {
            diceRoll = 0;
            learnPercentage = 0;
            return false;
        }

        return base.IsTriggered(user, victim, checkImprove, out diceRoll, out learnPercentage);
    }

    private static int ConvertSlotToOrderIndex(EquipmentSlots slot)
    {
        if (slot == EquipmentSlots.MainHand)
            return 0;
        if (slot == EquipmentSlots.OffHand)
            return 1;
        return 999;
    }
}
