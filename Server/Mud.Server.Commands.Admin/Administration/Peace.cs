using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("peace", "Admin")]
[Syntax("[cmd]")]
[Help(
@"[cmd] causes all characters in a room to stop fighting. It also strips the
AGGRESSIVE bit from mobiles.")]
public class Peace : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated()];

    public override void Execute(IActionInput actionInput)
    {
        foreach (var character in Impersonating.Room.People)
        {
            character.StopFighting(true);
            // Needed ?
            //if (character is INonPlayableCharacter npc && npc.ActFlags.HasFlag(ActFlags.Aggressive))
            //    npc.Remove
        }
        Actor.Send("Ok.");
    }
}
