using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("wield", "Item", "Equipment"), MinPosition(Positions.Resting)]
[Syntax("[cmd] <weapon>")]
[Help(
@"[cmd] will take an weapon from inventory and start using it
as equipment.")]
public class Wield : WearCharacterGameActionBase
{
    public Wield(IWiznet wiznet)
        : base(wiznet)
    {
    }

    protected IItem What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Wield what?";
        What = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (What == null)
            return StringHelpers.ItemInventoryNotFound;
        if (What.WearLocation == WearLocations.None)
            return "It cannot be wielded.";
        if (What is not IItemWeapon)
            return "Only weapons can be wielded.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        WearItem(What, true);
        Actor.Recompute();
    }
}
