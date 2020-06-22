using Mud.Server.Blueprints.Area;
using Mud.Server.Interfaces.Area;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    internal class AreaManagerMock : IAreaManager
    {
        private readonly List<IArea> _areas = new List<IArea>();
        private readonly List<AreaBlueprint> _areaBlueprints = new List<AreaBlueprint>();

        public IReadOnlyCollection<AreaBlueprint> AreaBlueprints => _areaBlueprints;

        public AreaBlueprint GetAreaBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public void AddAreaBlueprint(AreaBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IArea> Areas => _areas;

        public IArea AddArea(Guid guid, AreaBlueprint blueprint)
        {
            throw new NotImplementedException();
        }
    }
}
