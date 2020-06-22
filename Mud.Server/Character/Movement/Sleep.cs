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
    [CharacterCommand("sleep", "Movement", MinPosition = Positions.Sleeping)]
    [Syntax(
          "[cmd]",
          "[cmd] <furniture>")]
    public class Sleep : CharacterGameAction
    {
        public IItemFurniture What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Position == Positions.Fighting)
                return "Maybe you should finish fighting first?";
            if (Actor.Position == Positions.Standing)
                return "You are already sleeping.";

            // If already on a furniture and no parameter specified, use that furniture
            // Search valid furniture if any
            if (actionInput.Parameters.Length != 0)
            {
                IItem item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
                if (item == null)
                    return StringHelpers.ItemNotFound;

                What = item as IItemFurniture;
            }
            else
                What = Actor.Furniture;

            // Check furniture validity
            if (What != null)
            {
                if (!What.CanSleep)
                    return "You can't sleep on that.";

                // If already on furniture, don't count
                if (What != Actor.Furniture && 1 + What.People.Count() > What.MaxPeople)
                    return Actor.ActPhrase("There is no more room on {0} for you.", What);
            }

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.ChangeFurniture(What);

            // Change position
            if (What == null)
                Actor.Act(ActOptions.ToAll, "{0:N} go{0:v} to sleep.", Actor);
            if (What.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                Actor.Act(ActOptions.ToAll, "{0:N} go{0:v} sleep at {1}.", Actor, What);
            else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                Actor.Act(ActOptions.ToAll, "{0:N} go{0:v} sleep on {1}.", Actor, What);
            else if (What.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                Actor.Act(ActOptions.ToAll, "{0:N} go{0:v} sleep in {1}.", Actor, What);
            Actor.ChangePosition(Positions.Sleeping);
        }
    }
}
