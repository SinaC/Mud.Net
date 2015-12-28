using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        protected virtual bool Move(ServerOptions.ExitDirections direction, bool follow = false) // if follow is true, 'this' is not responsible for the move
        {
            IRoom fromRoom = Room;
            IExit exit = fromRoom.Exit(direction);
            IRoom toRoom = exit == null ? null : exit.Destination;

            // TODO: act_move.C:133
            // drunk
            // exit flags such as climb, door closed, ...
            // private room, size, swim room, guild room

            if (ControlledBy != null && ControlledBy.Room == Room) // Slave cannot leave a room without Master
                Send("What?  And leave your beloved master?");
            else if (exit == null || toRoom == null) // Check if existing exit
            {
                Send("You almost goes {0}, but suddenly realize that there's no exit there.", direction);
                Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction );
            }
            else
            {
                Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction);

                ChangeRoom(toRoom);
                // TODO: autolook ?

                Act(ActOptions.ToRoom, "{0} has arrived", this);

                // Followers: no circular follows
                if (fromRoom != toRoom)
                {
                    if (Slave != null)
                    {
                        Slave.Send("You follow {0}", Name);
                        (Slave as Character).Move(direction, true); // TODO: awful hack
                    }
                }
            }
            return true;
        }

        [Command("north")]
        protected virtual bool DoNorth(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.North);
        }

        [Command("east")]
        protected virtual bool DoEast(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.East);
        }

        [Command("south")]
        protected virtual bool DoSouth(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.South);
        }

        [Command("west")]
        protected virtual bool DoWest(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.West);
        }

        [Command("up")]
        protected virtual bool DoUp(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Up);
        }

        [Command("down")]
        protected virtual bool DoDown(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Down);
        }

        [Command("northeast")]
        [Command("ne")]
        protected virtual bool DoNorthEast(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.North);
        }

        [Command("northwest")]
        [Command("nw")]
        protected virtual bool DoNorthWest(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthWest);
        }

        [Command("southeast")]
        [Command("se")]
        protected virtual bool DoSouthEast(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthEast);
        }

        [Command("southwest")]
        [Command("sw")]
        protected virtual bool DoSouthWest(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthWest);
        }
    }
}
