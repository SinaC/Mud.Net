using Mud.Server.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("gossip", "Communication")]
    [PlayerCommand("ooc", "Communication")]
    [Syntax("[cmd] <message>")]
    public class Gossip : CommunicationGameActionBase
    {
        public Gossip(IPlayerManager playerManager)
            : base(playerManager)
        {
        }

        protected override string NoParamMessage => "Gossip what ?";
        protected override string ActorSendPattern => "%m%You gossip '%M%{0}%m%'%x%";
        protected override string OtherSendPattern => "%m%{0} gossips '%M%{1}%m%'%x%";
    }
}
