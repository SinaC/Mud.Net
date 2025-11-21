using Mud.Domain;

namespace Mud.Server.Blueprints.Reset;

public class DoorReset : ResetBase
{
    public ExitDirections ExitDirection { get; set; }
    public int Operation { get; set; } // 0: remove closed/locked  1: set closed, remove locked  2: set closed/locked  TODO
}
