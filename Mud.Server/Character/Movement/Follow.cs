using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("follow", "Group", "Movement")]
    [Syntax(
           "[cmd]",
           "[cmd] <character>")]
    public class Follow : CharacterGameAction
    {
        public bool DisplayLeader { get; protected set; }
        public ICharacter Whom { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;


            if (actionInput.Parameters.Length == 0)
            {
                if (Actor.Leader == null)
                    return "You are not following anyone.";
                DisplayLeader = true;
                return null;
            }

            // search target
            Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0]);
            if (Whom == null)
                return "They aren't here.";

            // follow ourself -> cancel follow
            if (Whom == this)
            {
                if (Actor.Leader == null)
                    return "You already follow yourself.";
                return null;
            }

            // check cycle
            ICharacter next = Whom.Leader;
            while (next != null)
            {
                if (next == this)
                    return Actor.ActPhrase("You can't follow {0:N}.", Whom);
                next = next.Leader;
            }

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (DisplayLeader)
            {
                Actor.Act(ActOptions.ToCharacter, "You are following {0:N}.", Actor.Leader);
                return;
            }
            if (Whom == Actor)
            {
                Actor.Leader.RemoveFollower(Actor);
                return;
            }

            Whom.Leader?.RemoveFollower(Actor);
            Whom.AddFollower(Actor);
        }
    }
}
