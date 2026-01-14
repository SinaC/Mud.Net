using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("rfind", "Information")]
[Syntax("[cmd] <room>")]
public class Rfind : AdminGameAction
{
    private IRoomManager RoomManager { get; }

    public Rfind(IRoomManager roomManager)
    {
        RoomManager = roomManager;
    }

    protected ICommandParameter Pattern { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Pattern = actionInput.Parameters[0];

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Searching rooms '{Pattern.Value}'");
        var rooms = FindHelpers.FindAllByName(RoomManager.Rooms, Pattern).OrderBy(x => x.Blueprint.Id).ToList();
        if (rooms.Count == 0)
            sb.AppendLine("No matches");
        else
        {
            sb.AppendLine("Id       DisplayName                              Area");
            sb.AppendLine("------------------------------------------------------");
            foreach (var room in rooms)
                sb.AppendLine($"{room.Blueprint.Id,-8} {room.DisplayName,-40} {room.Area.DisplayName}");
        }
        Actor.Page(sb);
    }
}
