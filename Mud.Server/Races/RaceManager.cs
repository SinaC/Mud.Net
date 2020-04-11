using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Server.Helpers;

namespace Mud.Server.Races
{
    public class RaceManager : IRaceManager
    {
        private readonly List<IRace> _races;

        public RaceManager()
        {
            // Get races using reflection
            Type iRaceType = typeof (IRace);
            _races = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iRaceType.IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<IRace>()
                .ToList();
        }
        
        #region IRaceManager

        public IEnumerable<IRace> Races => _races;

        public IRace this[string name] => _races.FirstOrDefault(x => FindHelpers.StringEquals(x.Name, name));

        #endregion
    }
}
