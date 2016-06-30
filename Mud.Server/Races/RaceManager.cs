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

        #region Singleton

        private static readonly Lazy<RaceManager> Lazy = new Lazy<RaceManager>(() => new RaceManager());

        public static IRaceManager Instance => Lazy.Value;

        private RaceManager()
        {
            // Get races using reflection
            Type iRaceType = typeof (IRace);
            _races = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iRaceType.IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<IRace>()
                .ToList();
        }

        #endregion

        #region IRaceManager

        public IReadOnlyCollection<IRace> Races => _races.AsReadOnly();

        public IRace this[string name]
        {
            get { return _races.FirstOrDefault(x => FindHelpers.StringEquals(x.Name, name)); }
        }

        #endregion
    }
}
