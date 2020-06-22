using Mud.Server.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("question", "Communication")]
    [Syntax("[cmd] <message>")]
    public class Question : CommunicationGameActionBase
    {
        public Question(IPlayerManager playerManager)
           : base(playerManager)
        {
        }

        protected override string NoParamMessage => "Ask what ?";
        protected override string ActorSendPattern => "%y%You question '{0}'%x%";
        protected override string OtherSendPattern => "%y%{0} questions '{1}'%x%";
    }
}
