using Mud.Logger;
using Mud.Server.Blueprints.Area;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Area;

namespace Mud.Server.Area;

public class AreaManager : IAreaManager
{
    private IWiznet Wiznet { get; }

    private readonly Dictionary<int, AreaBlueprint> _areaBlueprints;
    private readonly List<IArea> _areas;

    public AreaManager(IWiznet wiznet)
    {
        Wiznet = wiznet;

        _areaBlueprints = [];
        _areas = [];
    }

    public IReadOnlyCollection<AreaBlueprint> AreaBlueprints
        => _areaBlueprints.Values.ToList().AsReadOnly();
    public AreaBlueprint? GetAreaBlueprint(int id)
    {
        _areaBlueprints.TryGetValue(id, out var blueprint);
        return blueprint;
    }

    public void AddAreaBlueprint(AreaBlueprint blueprint)
    {
        if (_areaBlueprints.ContainsKey(blueprint.Id))
            Log.Default.WriteLine(LogLevels.Error, "Area blueprint duplicate {0}!!!", blueprint.Id);
        else
            _areaBlueprints.Add(blueprint.Id, blueprint);
    }

    public IEnumerable<IArea> Areas
        => _areas;

    public IArea AddArea(Guid guid, AreaBlueprint blueprint)
    {
        IArea area = new Area(Wiznet, guid, blueprint);
        _areas.Add(area);
        return area;
    }
}
