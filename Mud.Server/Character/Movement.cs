using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mud.Logger;

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
                string message = String.Format("{0} looks like {1}'s about to go {2}, but suddenly stops short and looks confused.", Name, "he" /*TODO: sex*/, direction);
                foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                    character.Send(message);
            }
            else
            {
                string leaveMessage = String.Format("{0} leaves {1}.", Name, direction);
                foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                    character.Send(leaveMessage);

                ChangeRoom(toRoom);
                // TODO: autolook ?

                string enterMessage = String.Format("{0} has arrived", Name);
                foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                    character.Send(enterMessage);

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
        protected virtual bool North(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.North);
        }

        [Command("east")]
        protected virtual bool East(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.East);
        }

        [Command("south")]
        protected virtual bool South(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.South);
        }

        [Command("west")]
        protected virtual bool West(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.West);
        }

        [Command("up")]
        protected virtual bool Up(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Up);
        }

        [Command("down")]
        protected virtual bool Down(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.Down);
        }

        [Command("northeast")]
        [Command("ne")]
        protected virtual bool NorthEast(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.North);
        }

        [Command("northwest")]
        [Command("nw")]
        protected virtual bool NorthWest(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthWest);
        }

        [Command("southeast")]
        [Command("se")]
        protected virtual bool SouthEast(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthEast);
        }

        [Command("southwest")]
        [Command("sw")]
        protected virtual bool SouthWest(string rawParameters, CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthWest);
        }
    }
}
