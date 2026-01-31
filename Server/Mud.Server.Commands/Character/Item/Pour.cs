using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("pour", "Drink")]
[Syntax(
        "[cmd] <container> out",
        "[cmd] <container> <container>",
        "[cmd] <container> <character>")]
[Help(
@"[cmd] transfers a liquid to a container, or empties one.
You can also pour from an object into something a character is holding.")]
public class Pour : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastTwoArguments { Message = "Pour what into what ?" }];

    private IItemDrinkContainer SourceContainer { get; set; } = default!;
    private ICharacter TargetCharacter { get; set; } = default!;
    private IItemDrinkContainer TargetContainer { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // search source container
        var item = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[0]);
        if (item == null)
            return "You don't have that item.";
        if (item is not IItemDrinkContainer sourceContainer)
            return "That's not a drink container.";
        SourceContainer = sourceContainer;

        // pour out
        if (StringCompareHelpers.StringEquals("out", actionInput.Parameters[1].Value))
        {
            if (SourceContainer.IsEmpty)
                return "It's already empty.";
            return null;
        }

        // pour into another container on someone's hand or here
        var targetItem = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[1]);
        if (targetItem == null)
        {
            TargetCharacter = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[1])!;
            if (TargetCharacter == null)
                return "Pour into what?";
            targetItem = TargetCharacter.GetEquipment(EquipmentSlots.OffHand);
            if (targetItem == null)
                return "They aren't holding anything.";
        }

        // destination item found
        if (targetItem is not IItemDrinkContainer targetContainer)
            return "You can only pour into other drink containers.";
        TargetContainer = targetContainer;
        if (TargetContainer == SourceContainer)
            return "You cannot change the laws of physics!";
        if (!TargetContainer.IsEmpty && TargetContainer.LiquidName != SourceContainer.LiquidName)
            return "They don't hold the same liquid.";
        if (SourceContainer.IsEmpty)
            return Actor.ActPhrase("There's nothing in {0} to pour.", SourceContainer);
        if (TargetContainer.LiquidLeft >= TargetContainer.MaxLiquid)
            return Actor.ActPhrase("{0} is already filled to the top.", TargetContainer);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // pour out
        if (TargetContainer == null)
        {
            SourceContainer.Pour();
            SourceContainer.Recompute();
            Actor.Act(ActOptions.ToAll, "{0:N} invert{0:v} {1}, spilling {2} all over the ground.", Actor, SourceContainer, SourceContainer.LiquidName ?? "mysterious liquid");
            return;
        }

        // pour into another container on someone's hand or here
        int amount = Math.Min(SourceContainer.LiquidLeft, TargetContainer.MaxLiquid - TargetContainer.LiquidLeft);
        TargetContainer.Fill(SourceContainer.LiquidName, amount);
        if (SourceContainer.IsPoisoned) // copy poison or not poisoned
            TargetContainer.Poison();
        else
            TargetContainer.Cure();
        TargetContainer.Recompute();
        SourceContainer.Recompute();
        //
        if (TargetCharacter == null)
            Actor.Act(ActOptions.ToAll, "{0:N} pour{0:v} {1} from {2} into {3}.", Actor, SourceContainer.LiquidName ?? "mysterious liquid", SourceContainer, TargetContainer);
        else
        {
            TargetCharacter.Act(ActOptions.ToCharacter, "{0:N} pours you some {1}.", Actor, SourceContainer.LiquidName ?? "mysterious liquid");
            TargetCharacter.Act(ActOptions.ToRoom, "{0:N} pour{0:v} some {1} for {2}", Actor, SourceContainer.LiquidName ?? "mysterious liquid", TargetCharacter);
        }
    }
}
