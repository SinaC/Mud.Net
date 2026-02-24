namespace Mud.Server.MobProgram.Interfaces;

public interface IMobProgram
{
    public Blueprints.MobProgram.MobProgramBlueprint Blueprint { get; }
    public INode[] Nodes { get; }
}
