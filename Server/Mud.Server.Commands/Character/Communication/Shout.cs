using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("shout", "Communication"), MinPosition(Positions.Resting), NoArgumentGuard("Shout what ?")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] sends a message to all awake players in the world.  To curb excessive
shouting, [cmd] imposes a three-second delay on the shouter.")]
public class Shout : CharacterGameAction
{
    private ICommandParser CommandParser { get; }
    private IPlayerManager PlayerManager { get; }

    public Shout(ICommandParser commandParser, IPlayerManager playerManager)
    {
        CommandParser = commandParser;
        PlayerManager = playerManager;
    }

    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating!), "{0:N} shout{0:v} '{1}'", Actor, What);
        Actor.SetGlobalCooldown(12);
    }
}
