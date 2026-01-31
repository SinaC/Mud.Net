using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("wield", "Item", "Equipment")]
[Syntax("[cmd] <weapon>")]
[Help(
@"[cmd] will take an weapon from inventory and start using it
as equipment.")]
public class Wield : WearCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Wield what ?" }];

    public Wield(IWiznet wiznet)
        : base(wiznet)
    {
    }

    private IItemWeapon Weapon { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var what = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (what == null)
            return StringHelpers.ItemInventoryNotFound;
        if (what.WearLocation == WearLocations.None)
            return "It cannot be wielded.";
        if (what is not IItemWeapon weapon)
            return "Only weapons can be wielded.";
        Weapon = weapon;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        WearItem(Weapon, true);
        Actor.Recompute();
    }
}
