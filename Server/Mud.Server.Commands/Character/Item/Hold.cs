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

[CharacterCommand("hold", "Item", "Equipment")]
[Syntax("[cmd] <item>")]
[Help(
@"[CMD] will take a light source, a wand, or a staff from inventory
and start using it as equipment.")]
public class Hold : WearCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Hold what ?" }];

    public Hold(IWiznet wiznet)
        : base(wiznet)
    {
    }

    private IItem What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return null;

        What = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
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
