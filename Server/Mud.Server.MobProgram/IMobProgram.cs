using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

public class MobProgram : IMobProgram
{
    public required Blueprints.MobProgram.MobProgramBlueprint Blueprint { get; set; }

    public required INode[] Nodes { get; set; }
}
