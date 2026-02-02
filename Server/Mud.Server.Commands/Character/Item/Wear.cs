using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("wear", "Item", "Equipment", Priority = 50)]
[Syntax(
        "[cmd] <item>",
        "[cmd] all")]
[Help(
@"[cmd] will take an armor from inventory and start using it
as equipment.
[cmd] ALL will attempt to HOLD, WEAR, or WIELD each suitable item in your
inventory.
[cmd] ALL.object will attempt to HOLD, WEAR or WIELD each suitable item
in your inventory with the same name")]
public class Wear : WearCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Wear, wield or hold what ?" }];

    public Wear(IWiznet wiznet)
        : base(wiznet)
    {
    }

    private IItem[] What { get; set; } = default!;
    private bool Replace { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var whatParameter = actionInput.Parameters[0];
        // wear all, wear all.item
        if (actionInput.Parameters[0].IsAll)
        {
            // We have to clone list because it'll be modified when wearing an item
            What = !whatParameter.IsAllOnly
                // get all.item
                ? FindHelpers.FindAllByName(Actor.Inventory.Where(Actor.CanSee), whatParameter).ToArray()
                // get all
                : Actor.Inventory.Where(Actor.CanSee).ToArray();
            if (What.Length == 0)
                return StringHelpers.ItemInventoryNotFound;
            Replace = false;
            return null;
        }
        // wear item
        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), whatParameter);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;
        What = [item];
        Replace = true;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var recomputeNeeded = false;
        foreach (var item in What)
            recomputeNeeded |= WearItem(item, Replace);
        if (recomputeNeeded)
            Actor.Recompute();
    }
}
