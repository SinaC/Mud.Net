using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Server.Common;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race
{
    public class RaceManager : IRaceManager
    {
        private readonly List<IRace> _races;

        public RaceManager()
        {
            // Get races using reflection
            Type iPlayableRaceType = typeof (IRace);
            _races = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iPlayableRaceType.IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<IRace>()
                .ToList();
        }
        
        #region IRaceManager

        public IEnumerable<IPlayableRace> PlayableRaces => Races.OfType<IPlayableRace>();

        public IEnumerable<IRace> Races => _races;

        public IRace this[string name] => _races.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));

        #endregion
    }
}
