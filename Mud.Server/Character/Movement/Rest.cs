using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("rest", "Movement", MinPosition = Positions.Sleeping)]
    [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
    public class Rest : CharacterGameAction
    {
        public IItemFurniture What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Position == Positions.Fighting)
                return "Maybe you should finish fighting first?";
            if (Actor.Position == Positions.Resting)
                return "You are already resting.";
            if (Actor.Position == Positions.Sleeping && Actor.CharacterFlags.HasFlag(CharacterFlags.Sleep))
                return "You can't wake up!";

            // Search valid furniture if any
            if (actionInput.Parameters.Length > 0)
            {
                IItem item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
                if (item == null)
                    return StringHelpers.ItemNotFound;

                IItemFurniture furniture = item as IItemFurniture;
                if (furniture == null || !furniture.CanRest)
                    return "You can't rest on that.";

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                    return Actor.ActPhrase("There is no more room on {0}.", furniture);
                What = furniture;
                return null;
            }

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.ChangeFurniture(What);

            // Change position
            if (Actor.Position == Positions.Sleeping)
            {
                if (What == null)
                    Actor.Act(ActOptions.ToAll, "{0:N} wake{0:v} and start{0:v} resting.", Actor);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Actor.Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} at {1}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Actor.Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} on {1}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Actor.Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} in {1}.", Actor, What);
            }
            else if (Actor.Position == Positions.Sitting)
            {
                if (What == null)
                    Actor.Act(ActOptions.ToRoom, "{0;N} rest{0:v}.", Actor);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Actor.Act(ActOptions.ToAll, "{0:N} rest{0:v} at {1}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Actor.Act(ActOptions.ToAll, "{0:N} rest{0:v} on {1}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Actor.Act(ActOptions.ToAll, "{0:N} rest{0:v} in {1}.", Actor, What);
            }
            else if (Actor.Position == Positions.Standing)
            {
                if (What == null)
                    Actor.Act(ActOptions.ToAll, "{0:N} sit{0:v} down and rest{0:v}.", Actor);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Actor.Act(ActOptions.ToAll, "{0:N} sit{0:v} down at {1} and rest{0:v}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Actor.Act(ActOptions.ToAll, "{0:N} sit{0:v} on {1} and rest{0:v}.", Actor, What);
                else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Actor.Act(ActOptions.ToAll, "{0:N} rest{0:v} in {1}.", Actor, What);
            }
            Actor.ChangePosition(Positions.Resting);
        }
    }
}
