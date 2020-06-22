using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("enter", "Movement", MinPosition = Positions.Standing)]
    [Syntax("[cmd] <portal>")]
    public class Enter : CharacterGameAction
    {
        public IItemPortal What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Fighting != null)
                return ""; // no specific message
            if (actionInput.Parameters.Length == 0)
                return "Nope, can't do it.";
            IItem item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.ItemNotFound;
            What = item as IItemPortal;
            if (What == null)
                return "You can't seem to find a way in.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Enter(What, true);
        }
    }
}
