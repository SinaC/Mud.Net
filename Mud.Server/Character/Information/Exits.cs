using System.Text;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("exits", "Information", MinPosition = Positions.Resting)]
    public class Exits : InformationCharacterGameActionBase
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            AppendExits(sb, Actor.Room, false);
            Actor.Send(sb);
        }
    }
}
