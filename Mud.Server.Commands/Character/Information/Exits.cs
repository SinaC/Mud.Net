using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("exits", "Information", Priority = 10, MinPosition = Positions.Resting)]
[Help(
@"Tells you the visible exits of the room you are in. Not all exits are visible.
You can use the 'bump' technique to find hidden exits. (Try to walk in a
certain direction and see what you bump into).")]
public class Exits : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        Actor.Room.AppendExits(sb, Actor, false);
        Actor.Send(sb);
    }
}
