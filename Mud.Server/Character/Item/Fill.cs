﻿using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("fill", "Drink", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <container>")]
    public class Fill : CharacterGameAction
    {
        public IItemFountain Fountain { get; protected set; }
        public IItemDrinkContainer DrinkContainer { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Fill what?";

            // search drink container
            IItem item = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[0]);
            if (item == null)
                return "You do not have that item.";

            // search fountain
            IItemFountain fountain = Actor.Room.Content.OfType<IItemFountain>().FirstOrDefault();
            if (fountain == null)
                return "There is no fountain here!";

            // drink container?
            IItemDrinkContainer drinkContainer = item as IItemDrinkContainer;
            if (drinkContainer == null)
                return "You can't fill that.";

            // same liquid ?
            if (!drinkContainer.IsEmpty && drinkContainer.LiquidName != fountain.LiquidName)
                return "There is already another liquid in it.";

            // full
            if (drinkContainer.LiquidLeft >= drinkContainer.MaxLiquid)
                return "Your container is full.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(ActOptions.ToAll, "{0:N} fill{0:v} {1} with {2} from {3}.", Actor, DrinkContainer, Fountain.LiquidName, Fountain);
            DrinkContainer.Fill(Fountain.LiquidName, DrinkContainer.MaxLiquid);
            DrinkContainer.Recompute();
        }
    }
}
