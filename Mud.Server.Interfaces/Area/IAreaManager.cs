using Mud.Server.Blueprints.Area;
using System;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Area
{
    public interface IAreaManager
    {
        IReadOnlyCollection<AreaBlueprint> AreaBlueprints { get; }

        AreaBlueprint GetAreaBlueprint(int id);

        void AddAreaBlueprint(AreaBlueprint blueprint);

        IEnumerable<IArea> Areas { get; }

        IArea AddArea(Guid guid, AreaBlueprint blueprint);
    }
}
