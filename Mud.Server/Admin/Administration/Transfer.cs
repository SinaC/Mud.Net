﻿using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("transfer", "Admin")]
    [Syntax(
            "[cmd] <character> (if impersonated)",
            "[cmd] <character> <location>")]
    public class Transfer : AdminGameAction
    {
        private IWorld World { get; }

        public IRoom Where { get; protected set; }
        public ICharacter Whom { get; protected set; }

        public Transfer(IWorld world)
        {
            World = world;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();
            if (Actor.Impersonating == null && actionInput.Parameters.Length == 1)
                return "Transfer without specifying location can only be used when impersonating.";

            // TODO: IsAll ?

            IRoom where;
            if (Actor.Impersonating != null)
                where = actionInput.Parameters.Length == 1
                    ? Actor.Impersonating.Room
                    : FindHelpers.FindLocation(Actor.Impersonating, actionInput.Parameters[1]);
            else
                where = FindHelpers.FindLocation(actionInput.Parameters[1]);
            if (where == null)
                return "No such location.";
            if (where.IsPrivate)
                return "That room is private right now.";

            ICharacter whom = Actor.Impersonating != null
                ? FindHelpers.FindChararacterInWorld(Actor.Impersonating, actionInput.Parameters[0])
                : FindHelpers.FindByName(World.Characters, actionInput.Parameters[0]);
            if (whom == null)
                return StringHelpers.CharacterNotFound;

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Whom.Fighting != null)
                Whom.StopFighting(true);
            Whom.Act(ActOptions.ToRoom, "{0:N} disappears in a mushroom cloud.", Whom);
            Whom.ChangeRoom(Where);
            Whom.Act(ActOptions.ToRoom, "{0:N} appears from a puff of smoke.", Whom);
            if (Whom != Actor.Impersonating)
            {
                if (Actor.Impersonating != null)
                    Whom.Act(ActOptions.ToCharacter, "{0:N} has transferred you.", Actor.Impersonating);
                else
                    Whom.Act(ActOptions.ToCharacter, "Someone has transferred you.");
            }
            Whom.AutoLook();

            Actor.Send("Ok");
        }
    }
}
