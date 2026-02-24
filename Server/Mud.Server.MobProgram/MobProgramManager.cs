using Microsoft.Extensions.Logging;
using Mud.Blueprints.MobProgram;
using Mud.Common.Attributes;
using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

[Export(typeof(IMobProgramManager)), Shared]
public class MobProgramManager : IMobProgramManager
{
    private ILogger<MobProgramManager> Logger { get; }
    private IMobProgramParser MobProgramParser { get; }

    private readonly Dictionary<int, IMobProgram> _mobPrograms;

    public MobProgramManager(ILogger<MobProgramManager> logger, IMobProgramParser mobProgramParser)
    {
        _mobPrograms = [];
        Logger = logger;
        MobProgramParser = mobProgramParser;
    }

    public IReadOnlyCollection<IMobProgram> MobPrograms => _mobPrograms.Values;

    public IMobProgram? GetMobProgram(int id)
        => _mobPrograms.GetValueOrDefault(id);

    public void AddMobProgram(MobProgramBlueprint mobProgramBlueprint)
    {
        if (_mobPrograms.ContainsKey(mobProgramBlueprint.Id))
        {
            Logger.LogError("MobProgram duplicate {mobProgramId}!!!", mobProgramBlueprint.Id);
            return;
        }

        var nodes = MobProgramParser.Parse(mobProgramBlueprint.Code);
        var mobProgram = new MobProgram
        {
            Blueprint = mobProgramBlueprint,
            Nodes = nodes.ToArray()
        };
        _mobPrograms.Add(mobProgramBlueprint.Id, mobProgram);
    }
}
