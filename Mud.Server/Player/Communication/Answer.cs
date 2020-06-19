using Mud.Server.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("answer", "Communication")]
    [Syntax("[cmd] <message>")]
    public class Answer : CommunicationGameActionBase
    {
        public Answer(IPlayerManager playerManager)
           : base(playerManager)
        {
        }

        protected override string NoParamMessage => "Answer what ?";
        protected override string ActorSendPattern => "%y%You answer '{0}'%x%";
        protected override string OtherSendPattern => "%y%{0} answers '{1}'%x%";
    }
}
