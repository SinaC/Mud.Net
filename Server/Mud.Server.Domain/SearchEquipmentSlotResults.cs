namespace Mud.Server.Domain;

public enum SearchEquipmentSlotResults
{
    Found,
    NoWearLocation,
    UnknownWearLocation,
    NotFound,
    NotEmpty,
    NoFreeMainHand,
    NoFreeOffHand,
    NoFreeMainOrOffHand, // searched to equip 1H in mainhand or offhand
    NoFreeMainAndOffHand // searched to equip 2H in mainhand + offhand
}
