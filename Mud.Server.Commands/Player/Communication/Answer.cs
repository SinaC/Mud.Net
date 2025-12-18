using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("answer", "Communication")]
[Syntax("[cmd] <message>")]
[Help(
@"With these channels, you can ask questions to other players such newbie 
helper. You can turn it off by simply typing QUESTION.")]
public class Answer : CommunicationGameActionBase
{
    public Answer(ICommandParser commandParser, IPlayerManager playerManager)
       : base(commandParser, playerManager)
    {
    }

    protected override string NoParamMessage => "Answer what ?";
    protected override string ActorSendPattern => "%y%You answer '{0}'%x%";
    protected override string OtherSendPattern => "%y%{0} answers '{1}'%x%";
}
