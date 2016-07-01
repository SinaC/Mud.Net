using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("north", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.North);
        }

        [Command("east", Category = "Movement", Priority = 0)]
        protected virtual bool DoEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.East);
        }

        [Command("south", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.South);
        }

        [Command("west", Category = "Movement", Priority = 0)]
        protected virtual bool DoWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.West);
        }

        [Command("up", Category = "Movement", Priority = 0)]
        protected virtual bool DoUp(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.Up);
        }

        [Command("down", Category = "Movement", Priority = 0)]
        protected virtual bool DoDown(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.Down);
        }

        [Command("northeast", Category = "Movement", Priority = 1)]
        [Command("ne", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.NorthEast);
        }

        [Command("northwest", Category = "Movement", Priority = 1)]
        [Command("nw", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.NorthWest);
        }

        [Command("southeast", Category = "Movement", Priority = 1)]
        [Command("se", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.SouthEast);
        }

        [Command("southwest", Category = "Movement", Priority = 1)]
        [Command("sw", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.SouthWest);
        }
    }
}
