using System.Linq;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Communication
{
    [CharacterCommand("yell", "Communication", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <message>")]
    public class Yell : CharacterGameAction
    {
        public string What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Yell what?";

            What = CommandHelpers.JoinParameters(actionInput.Parameters);

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(Actor.Room.Area.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), "%G%{0:n} yell{0:v} '%x%{1}%G%'%x%", Actor, What);
        }
    }
}
