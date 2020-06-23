using System.Linq;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Character.Communication
{
    [CharacterCommand("shout", "Communication", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <message>")]
    public class Shout : CharacterGameAction
    {
        private IPlayerManager PlayerManager { get; }

        public string What { get; protected set; }

        public Shout(IPlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Shout what?";

            What = CommandHelpers.JoinParameters(actionInput.Parameters);

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), "{0:N} shout{0:v} '{1}'", Actor, What);
        }
    }
}
