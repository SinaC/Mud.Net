using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("pour", "Drink", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd] <container> out",
            "[cmd] <container> <container>",
            "[cmd] <container> <character>")]
    public class Pour : CharacterGameAction
    {
        public IItemDrinkContainer SourceContainer { get; protected set; }
        public ICharacter TargetCharacter { get; protected set; }
        public IItemDrinkContainer TargetContainer { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return "Pour what into what?";

            // search source container
            IItem item = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[0]);
            if (item == null)
                return "You don't have that item.";
            SourceContainer = item as IItemDrinkContainer;
            if (SourceContainer == null)
                return "That's not a drink container.";

            // pour out
            if (StringCompareHelpers.StringEquals("out", actionInput.Parameters[1].Value))
            {
                if (SourceContainer.IsEmpty)
                    return "It's already empty.";
                return null;
            }

            // pour into another container on someone's hand or here
            IItem targetItem = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[1]);
            if (targetItem == null)
            {
                TargetCharacter = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[1]);
                if (TargetCharacter == null)
                    return "Pour into what?";
                targetItem = TargetCharacter.GetEquipment(EquipmentSlots.OffHand);
                if (targetItem == null)
                    return "They aren't holding anything.";
            }

            // destination item found
            TargetContainer = targetItem as IItemDrinkContainer;
            if (TargetContainer == null)
                return "You can only pour into other drink containers.";
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
                Actor.Act(ActOptions.ToAll, "{0:N} invert{0:v} {1}, spilling {2} all over the ground.", Actor, SourceContainer, SourceContainer.LiquidName);
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
                Actor.Act(ActOptions.ToAll, "{0:N} pour{0:v} {1} from {2} into {3}.", Actor, SourceContainer.LiquidName, SourceContainer, TargetContainer);
            else
            {
                TargetCharacter.Act(ActOptions.ToCharacter, "{0:N} pours you some {1}.", Actor, SourceContainer.LiquidName);
                TargetCharacter.Act(ActOptions.ToRoom, "{0:N} pour{0:v} some {1} for {2}", Actor, SourceContainer.LiquidName, TargetCharacter);
            }
        }
    }
}
