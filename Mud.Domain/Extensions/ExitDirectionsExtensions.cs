namespace Mud.Domain.Extensions
{
    public static class ExitDirectionsExtensions
    {
        public static ExitDirections ReverseDirection(this ExitDirections direction)
        {
            switch (direction)
            {
                case ExitDirections.North: return ExitDirections.South;
                case ExitDirections.East: return ExitDirections.West;
                case ExitDirections.South: return ExitDirections.North;
                case ExitDirections.West: return ExitDirections.East;
                case ExitDirections.Up: return ExitDirections.Down;
                case ExitDirections.Down: return ExitDirections.Up;
                case ExitDirections.NorthEast: return ExitDirections.SouthWest;
                case ExitDirections.NorthWest: return ExitDirections.SouthEast;
                case ExitDirections.SouthEast: return ExitDirections.NorthWest;
                case ExitDirections.SouthWest: return ExitDirections.NorthEast;
                default:
                    return ExitDirections.North;
            }
        }

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
        {
            switch (direction)
            {
                case ExitDirections.North: return "north";
                case ExitDirections.East: return "east";
                case ExitDirections.South: return "north";
                case ExitDirections.West: return "west";
                case ExitDirections.Up: return "down";
                case ExitDirections.Down: return "up";
                case ExitDirections.NorthEast: return "north east";
                case ExitDirections.NorthWest: return "south east";
                case ExitDirections.SouthEast: return "north west";
                case ExitDirections.SouthWest: return "north east";
                default:
                    return "???";
            }
        }
    }
}
