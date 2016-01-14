namespace Mud.Server.Constants
{
    public static class ExitHelpers
    {
        public const int ExitCount = 10;

        public static ExitDirections ReverseDirection(ExitDirections direction)
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
    }

    public enum ExitDirections // MUST contains 'ExitCount' entries
    {
        North,
        East,
        South,
        West,
        Up,
        Down,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
    }
}
