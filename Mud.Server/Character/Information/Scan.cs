using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Character.Information;

[CharacterCommand("scan", "Information", MinPosition = Positions.Standing, NotInCombat = true)]
public class Scan : CharacterGameAction
{
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Room == null)
            return "You are nowhere.";

        if (Actor.Room.RoomFlags.IsSet("NoScan"))
            return "Your vision is clouded by a mysterious force.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new(1024);
        // Current room
        sb.AppendLine("Right here you see:");
        StringBuilder currentScan = ScanRoom(Actor.Room);
        if (currentScan.Length == 0)
            sb.AppendLine("None");// should never happen, 'this' is in the room
        else
            sb.Append(currentScan);
        // Scan in one direction for each distance, then starts with another direction
        foreach (var direction in EnumHelpers.GetValues<ExitDirections>())
        {
            IRoom currentRoom = Actor.Room; // starting point
            for (int distance = 1; distance < 4; distance++)
            {
                var exit = currentRoom[direction];
                var destination = exit?.Destination;
                if (destination == null)
                    break; // stop in that direction if no exit found or no linked room found
                if (destination.RoomFlags.IsSet("NoScan"))
                    break; // no need to scan further
                if (exit?.IsClosed == true)
                    break; // can't see thru closed door
                StringBuilder roomScan = ScanRoom(destination);
                if (roomScan.Length > 0)
                {
                    sb.AppendFormatLine("%c%{0} %r%{1}%x% from here you see:", distance, direction);
                    sb.Append(roomScan);
                }

                currentRoom = destination;
            }
        }
        Actor.Send(sb);
    }

    private StringBuilder ScanRoom(IRoom room)
    {
        StringBuilder peopleInRoom = new();
        foreach (ICharacter victim in room.People.Where(x => Actor.CanSee(x)))
            peopleInRoom.AppendFormatLine(" - {0}", victim.RelativeDisplayName(Actor));
        return peopleInRoom;
    }
}
