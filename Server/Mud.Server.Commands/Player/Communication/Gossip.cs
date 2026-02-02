using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("gossip", "Communication")]
[Alias("ooc")]
[Alias(".")]
[Syntax("[cmd] <message>")]
[Help(@"[cmd] sends a message to all players in the world (Out Of Character channel).")]
public class Gossip : CommunicationGameActionBase
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Gossip what ?" }];

    public Gossip(ICommandParser commandParser, IPlayerManager playerManager)
        : base(commandParser, playerManager)
    {
    }

    protected override string ActorSendPattern => "%m%You gossip '%M%{0}%m%'%x%";
    protected override string OtherSendPattern => "%m%{0} gossips '%M%{1}%m%'%x%";
}
