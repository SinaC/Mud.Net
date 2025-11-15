using Mud.Server.Blueprints.Area;

namespace Mud.Server.Interfaces.Area;

public interface IAreaManager
{
    IReadOnlyCollection<AreaBlueprint> AreaBlueprints { get; }

    AreaBlueprint? GetAreaBlueprint(int id);

    void AddAreaBlueprint(AreaBlueprint blueprint);

    IEnumerable<IArea> Areas { get; }

    IArea AddArea(Guid guid, AreaBlueprint blueprint);
}
