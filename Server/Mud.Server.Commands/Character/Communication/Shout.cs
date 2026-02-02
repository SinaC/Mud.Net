using Mud.Domain;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("shout", "Communication")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] sends a message to all awake players in the world.  To curb excessive
shouting, [cmd] imposes a three-second delay on the shouter.")]
public class Shout : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Shout what ?" }];

    private ICommandParser CommandParser { get; }
    private IPlayerManager PlayerManager { get; }

    public Shout(ICommandParser commandParser, IPlayerManager playerManager)
    {
        CommandParser = commandParser;
        PlayerManager = playerManager;
    }

    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
