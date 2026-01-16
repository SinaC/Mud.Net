using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("peace", "Admin"), MustBeImpersonated]
[Syntax("[cmd]")]
[Help(
@"[cmd] causes all characters in a room to stop fighting. It also strips the
AGGRESSIVE bit from mobiles.")]
public class Peace : AdminGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        foreach (ICharacter character in Impersonating.Room.People)
        {
            character.StopFighting(true);
            // Needed ?
            //if (character is INonPlayableCharacter npc && npc.ActFlags.HasFlag(ActFlags.Aggressive))
            //    npc.Remove
        }
        Actor.Send("Ok.");
    }
}
