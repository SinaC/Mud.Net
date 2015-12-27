using System;
using Mud.Logger;

namespace Mud.Server
{
    public class ServerOptions
    {
        #region Singleton

        private static readonly Lazy<ServerOptions> Lazy = new Lazy<ServerOptions>(() => new ServerOptions());

        public static ServerOptions Instance
        {
            get { return Lazy.Value; }
        }

        private ServerOptions()
        {
            PrefixForwardedMessages = true;
            ForwardSlaveMessages = false;
        }

        #endregion

        public enum ExitDirections
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

        public const int ExitCount = 10;

        public bool PrefixForwardedMessages { get; set; } // Add <IMP> or <CTRL> before forwarding a message
        public bool ForwardSlaveMessages { get; set; } // Forward messages received by a slaved character


        public ExitDirections ReverseDirection(ExitDirections direction)
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
                    Log.Default.WriteLine(LogLevels.Warning, "Invalid direction {0}", direction);
                    return ExitDirections.North;
            }
        }
    }
}
