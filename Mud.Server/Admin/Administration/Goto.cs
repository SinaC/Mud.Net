using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using System.Linq;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("goto", "Admin", MustBeImpersonated = true)]
    [Syntax("[cmd] <location>")]
    public class Goto : AdminGameAction
    {
        public IPlayableCharacter Impersonating { get; protected set; }
        public IRoom Where { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return null;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            Impersonating = Actor.Impersonating;

            Where = FindHelpers.FindLocation(Impersonating, actionInput.Parameters[0]);
            if (Where == null)
                return "No such location.";
            if (Where.IsPrivate && Where.People.Count() > 1)
                return "That room is private right now.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Impersonating.Fighting != null)
                Impersonating.StopFighting(true);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0:N} leaves in a swirling mist.", Impersonating); // Don't display 'Someone leaves ...' if Impersonating is not visible
            Impersonating.ChangeRoom(Where);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0:N} appears in a swirling mist.", Impersonating);
            Impersonating.AutoLook();
        }
    }
}
