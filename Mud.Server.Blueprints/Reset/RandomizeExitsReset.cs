namespace Mud.Server.Blueprints.Reset;

public class RandomizeExitsReset : ResetBase
{
    public int MaxDirections { get; set; } // max direction exit to randomize if set to 4: exit direction will be choosen between the 4 first directions (north, east, south, west)
}
