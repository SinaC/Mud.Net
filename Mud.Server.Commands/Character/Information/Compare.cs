using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("compare", "Information", MinPosition = Positions.Resting)]
[Alias("cmp")]
[Syntax(
    "[cmd] <object-1> <object-2>",
    "[cmd] <object>")]
[Help(
@"[cmd] compares two objects in your inventory.  If both objects are weapons,
it will report the one with the better average damage.  If both objects are
armor, it will report the one with the better armor class.
 
[cmd] with one argument compares an object in your inventory to the object
you are currently wearing or wielding of the same type.
 
[cmd] doesn't consider any special modifiers of the objects.")]
public class Compare : CharacterGameAction
{
    protected IItem Item1 { get; set; } = default!;
    protected IItem Item2 { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Compare what to what?";

        // search first item in inventory
        var what1 = actionInput.Parameters[0];
        var item1 = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), what1);
        if (item1 == null)
            return StringHelpers.ItemInventoryNotFound;
        Item1 = item1;

        // second parameter is non-empty, search second item in inventory
        if (actionInput.Parameters.Length > 1)
        {
            // search
            var what2 = actionInput.Parameters[1];
            var item2 = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), what2);
            if (item2 == null)
                return StringHelpers.ItemInventoryNotFound;
            Item2 = item2;
        }
        // second parameter is empty, search in equipement for a similar item
        else
        {
            // same item type and same equipement slot
            var item2 = Actor.Equipments.FirstOrDefault(x => x.Item is not null && Actor.CanSee(x.Item) && x.Item.GetType() == item1.GetType() && IsWearLocationMatching(item1.WearLocation, x.Item.WearLocation) && !item1.NoTake);
            if (item2?.Item == null)
                return "You aren't wearing anything comparable.";
            Item2 = item2.Item;
        }

        // we have found 2 items to compare
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Item1 == Item2)
            Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "You compare {0} to itself.  It looks about the same.", Item1);
        else if (Item1.GetType() != Item2.GetType())
            Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "You can't compare {0} and {1}.", Item1, Item2);
        else
        {
            // from here item1 has the same type as item2
            switch (Item1)
            {
                case IItemArmor:
                    CompareArmor();
                    break;
                case IItemWeapon:
                    CompareWeapon();
                    break;
                default:
                    Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "You can't compare {0} and {1}.", Item1, Item2);
                    break;
            }
        }
    }

    private void CompareArmor()
    {
        var value1 = CalculateCompareArmorValue((IItemArmor)Item1);
        var value2 = CalculateCompareArmorValue((IItemArmor)Item2);

        DisplayCompareResult(value1, value2);
    }

    private static int CalculateCompareArmorValue(IItemArmor armor)
        => armor.Bash + armor.Pierce + armor.Slash + armor.Exotic;

    private void CompareWeapon()
    {
        var value1 = CalculateCompareWeaponValue((IItemWeapon)Item1);
        var value2 = CalculateCompareWeaponValue((IItemWeapon)Item2);

        DisplayCompareResult(value1, value2);
    }

    private static int CalculateCompareWeaponValue(IItemWeapon weapon)
        => (1 + weapon.DiceCount) * weapon.DiceValue;

    private void DisplayCompareResult(int value1, int value2)
    {
        if (value1 == value2)
            Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "{0} and {1} look about the same.", Item1, Item2);
        else if (value1 > value2)
            Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "{0} looks better than {1}.", Item1, Item2);
        else
            Actor.Act(Interfaces.Character.ActOptions.ToCharacter, "{0} looks worse than {1}.", Item1, Item2);
    }

    private static bool IsWearLocationMatching(WearLocations loc1, WearLocations loc2)
    {
        // simple case
        if (loc1 == loc2)
            return true;
        // weapon case
        if ((loc1 == WearLocations.Wield || loc1 == WearLocations.Wield2H) && (loc2 == WearLocations.Wield || loc2 == WearLocations.Wield2H))
            return true;
        return false;
    }
}
