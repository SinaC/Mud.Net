using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("question", "Communication")]
[Alias("ask")]
[Syntax("[cmd] <message>")]
[Help(
@"With these channels, you can ask questions to other players such newbie 
helper. You can turn it off by simply typing [cmd].")]
public class Question : CommunicationGameActionBase
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Ask what ?" }];

    public Question(ICommandParser commandParser, IPlayerManager playerManager)
       : base(commandParser, playerManager)
    {
    }

    protected override string ActorSendPattern => "%y%You question '{0}'%x%";
    protected override string OtherSendPattern => "%y%{0} questions '{1}'%x%";
}
