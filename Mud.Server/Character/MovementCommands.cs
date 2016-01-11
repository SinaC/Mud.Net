using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Character
{
    public partial class Character
    {
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

        [Command("northeast", Priority = 1)]
        [Command("ne", Priority = 1)]
        protected virtual bool DoNorthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthEast);
        }

        [Command("northwest", Priority = 1)]
        [Command("nw", Priority = 1)]
        protected virtual bool DoNorthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.NorthWest);
        }

        [Command("southeast", Priority = 1)]
        [Command("se", Priority = 1)]
        protected virtual bool DoSouthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthEast);
        }

        [Command("southwest", Priority = 1)]
        [Command("sw", Priority = 1)]
        protected virtual bool DoSouthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ServerOptions.ExitDirections.SouthWest);
        }
    }
}
