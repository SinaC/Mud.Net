namespace Mud.Domain.Extensions;

public static class ExitDirectionsExtensions
{

    public static string ShortExitDirections(this ExitDirections exitDirections)
        => exitDirections switch
        {
            ExitDirections.North => "N",
            ExitDirections.East => "E",
            ExitDirections.South => "S",
            ExitDirections.West => "W",
            ExitDirections.Up => "U",
            ExitDirections.Down => "D",
            ExitDirections.NorthEast => "ne",
            ExitDirections.NorthWest => "nw",
            ExitDirections.SouthEast => "se",
            ExitDirections.SouthWest => "sw",
            _ => "?",
        };

    public static ExitDirections ReverseDirection(this ExitDirections direction)
        => direction switch
        {
            ExitDirections.North => ExitDirections.South,
            ExitDirections.East => ExitDirections.West,
            ExitDirections.South => ExitDirections.North,
            ExitDirections.West => ExitDirections.East,
            ExitDirections.Up => ExitDirections.Down,
            ExitDirections.Down => ExitDirections.Up,
            ExitDirections.NorthEast => ExitDirections.SouthWest,
            ExitDirections.NorthWest => ExitDirections.SouthEast,
            ExitDirections.SouthEast => ExitDirections.NorthWest,
            ExitDirections.SouthWest => ExitDirections.NorthEast,
            _ => ExitDirections.North,
        };

    public static bool TryFindDirection(string direction, out ExitDirections exitDirection)
    {
        exitDirection = ExitDirections.North;
        if (direction == "n" || direction == "north")
            exitDirection = ExitDirections.North;
        else if (direction == "e" || direction == "east")
            exitDirection = ExitDirections.East;
        else if (direction == "s" || direction == "south")
            exitDirection = ExitDirections.South;
        else if (direction == "w" || direction == "west")
            exitDirection = ExitDirections.West;
        else if (direction == "u" || direction == "up")
            exitDirection = ExitDirections.Up;
        else if (direction == "d" || direction == "down")
            exitDirection = ExitDirections.Down;
        else if (direction == "n" || direction == "north")
            exitDirection = ExitDirections.North;
        else if (direction == "ne" || direction == "northeast")
            exitDirection = ExitDirections.NorthEast;
        else if (direction == "nw" || direction == "northwest")
            exitDirection = ExitDirections.NorthWest;
        else if (direction == "se" || direction == "southeas")
            exitDirection = ExitDirections.SouthEast;
        else if (direction == "sw" || direction == "southwest")
            exitDirection = ExitDirections.SouthWest;
        else
            return false;
        return true;
    }

    public static string DisplayName(this ExitDirections direction)
        => direction switch
        {
            ExitDirections.North => "north",
            ExitDirections.East => "east",
            ExitDirections.South => "south",
            ExitDirections.West => "west",
            ExitDirections.Up => "up",
            ExitDirections.Down => "down",
            ExitDirections.NorthEast => "north east",
            ExitDirections.NorthWest => "north west",
            ExitDirections.SouthEast => "south east",
            ExitDirections.SouthWest => "south west",
            _ => "???",
        };
}
