using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("hold", "Item", "Equipment", MinPosition = Positions.Resting)]
[Syntax("[cmd] <item>")]
[Help(
@"[CMD] will take a light source, a wand, or a staff from inventory
and start using it as equipment.")]
public class Hold : WearCharacterGameActionBase
{
    public Hold(IWiznet wiznet)
        : base(wiznet)
    {
    }

    protected IItem What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return null;

        if (actionInput.Parameters.Length == 0)
            return "Wield what?";

        What = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0])!;
        if (What == null)
            return StringHelpers.ItemInventoryNotFound;

        if (What.WearLocation != WearLocations.Hold && What.WearLocation != WearLocations.Shield && What.WearLocation != WearLocations.Light)
            return "It cannot be hold.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        WearItem(What, true);
        Actor.Recompute();
    }
}
