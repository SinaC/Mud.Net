using Mud.Domain;

namespace Mud.Blueprints.MobProgram;

public class MobProgramExit : MobProgramBase
{
    public ExitDirections Direction { get; set; }
    public bool IsAll { get; set; }
}
