using Mud.Domain;

namespace Mud.Server.Blueprints.Reset
{
    public class DoorReset : ResetBase
    {
        public ExitDirections ExitDirection { get; set; }
        public ExitFlags ExitFlags { get; set; }
    }
}
