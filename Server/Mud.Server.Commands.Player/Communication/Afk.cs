using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("afk", "Communication")]
[Help(
@"Typing [cmd] puts your character in a tell-saving mode as follows: any tell
to you is stored in a special buffer, and can be seen later by typing
replay.  This is useful when you need to leave the mud for 5 or 10 minutes,
but don't want to miss tells. [cmd] shows up in your prompt until it is
turned off.")]
public class Afk : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        Actor.ToggleAfk();
    }
}
