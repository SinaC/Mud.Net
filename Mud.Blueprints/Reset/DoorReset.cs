using Mud.Domain;

namespace Mud.Blueprints.Reset;

public class DoorReset : ResetBase
{
    public ExitDirections ExitDirection { get; set; }
    public DoorOperations Operation { get; set; } // 0: remove closed/locked  1: set closed, remove locked  2: set closed/locked  TODO
}
