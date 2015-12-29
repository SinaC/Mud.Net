﻿using Mud.Server.Input;

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
                // Autolook if impersonated/incarnated
                if (ImpersonatedBy != null || IncarnatedBy != null)
                    DisplayRoom();

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

        [Command("north", Priority = 0)]
        protected virtual bool DoNorth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.North);
        }

        [Command("east", Priority = 0)]
        protected virtual bool DoEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.East);
        }

        [Command("south", Priority = 0)]
        protected virtual bool DoSouth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.South);
        }

        [Command("west", Priority = 0)]
        protected virtual bool DoWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.West);
        }

        [Command("up", Priority = 0)]
        protected virtual bool DoUp(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Up);
        }

        [Command("down", Priority = 0)]
        protected virtual bool DoDown(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Down);
        }

        [Command("northeast", Priority = 0)]
        [Command("ne", Priority = 0)]
        protected virtual bool DoNorthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthEast);
        }

        [Command("northwest", Priority = 0)]
        [Command("nw", Priority = 0)]
        protected virtual bool DoNorthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthWest);
        }

        [Command("southeast", Priority = 0)]
        [Command("se", Priority = 0)]
        protected virtual bool DoSouthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthEast);
        }

        [Command("southwest", Priority = 0)]
        [Command("sw", Priority = 0)]
        protected virtual bool DoSouthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthWest);
        }
    }
}