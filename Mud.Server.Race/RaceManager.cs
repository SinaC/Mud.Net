using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race
{
    public class RaceManager : IRaceManager
    {
        private readonly List<IRace> _races;

        public RaceManager(IAssemblyHelper assemblyHelper, IAbilityManager abilityManager)
        {
            // Get races using reflection
            Type iRaceType = typeof (IRace);
            Type iPlayableRaceType = typeof(IPlayableRace);
            _races = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && iRaceType.IsAssignableFrom(t))
                .Select(t => CreateInstance(t, iPlayableRaceType.IsAssignableFrom(t), abilityManager)))
                .ToList();
        }
        
        #region IRaceManager

        public IEnumerable<IPlayableRace> PlayableRaces => Races.OfType<IPlayableRace>();

        public IEnumerable<IRace> Races => _races;

        public IRace this[string name] => _races.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));

        #endregion

        private IRace CreateInstance(Type type, bool isPlayableRace, IAbilityManager abilityManager)
        {
            if (isPlayableRace)
                return Activator.CreateInstance(type, abilityManager) as IRace;
            return Activator.CreateInstance(type) as IRace;
        }
    }
}
