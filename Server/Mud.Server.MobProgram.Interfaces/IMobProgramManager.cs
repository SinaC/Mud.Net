namespace Mud.Server.MobProgram.Interfaces;

public interface IMobProgramManager
{
    IReadOnlyCollection<IMobProgram> MobPrograms { get; }

    IMobProgram? GetMobProgram(int id);

    void AddMobProgram(Blueprints.MobProgram.MobProgramBlueprint mobProgramBlueprint);
}
