using Mud.Container;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Affects
{
    public class AffectManager : IAffectManager
    {
        private readonly Dictionary<string, Type> _affectsByName;

        public AffectManager(IAssemblyHelper assemblyHelper)
        {
            Type iAffectType = typeof(IAffect);
            _affectsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iAffectType.IsAssignableFrom(t)))
                .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<AffectAttribute>() })
                .Where(x => x.attribute != null)
                .ToDictionary(x => x.attribute.Name, x => x.executionType);
        }

        public IAffect CreateInstance(string name)
        {
            if (!_affectsByName.TryGetValue(name, out var affectType))
            {
                Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} not found.", name);
                return null;
            }

            if (DependencyContainer.Current.GetRegistration(affectType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} not found in DependencyContainer.", affectType.FullName);
                return null;
            }

            IAffect instance = DependencyContainer.Current.GetInstance(affectType) as IAffect;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} cannot be create or is not of type {1}", affectType.FullName, typeof(IAffect).FullName);
                return null;
            }
            return instance;
        }
    }
}
