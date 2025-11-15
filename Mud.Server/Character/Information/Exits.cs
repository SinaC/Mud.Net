using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Character.Information;

[CharacterCommand("exits", "Information", MinPosition = Positions.Resting)]
public class Exits : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        Actor.Room.AppendExits(sb, Actor, false);
        Actor.Send(sb);
    }
}
