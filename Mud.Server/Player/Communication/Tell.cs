using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Linq;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("tell", "Communication")]
    [Syntax("[cmd] <player name> <message>")]
    public class Tell : TellGameActionBase
    {
        private IPlayerManager PlayerManager { get; }

        public IPlayer Whom { get; protected set; }
        public string What { get; protected set; }

        public Tell(IPlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return "Tell whom what ?";

            Whom = PlayerManager.GetPlayer(actionInput.Parameters[0], true);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;

            What = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            InnerTell(Whom, What);
        }
    }
}
