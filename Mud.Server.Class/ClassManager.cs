using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class
{
    public class ClassManager : IClassManager
    {
        private readonly List<IClass> _classes;

        public ClassManager(IAssemblyHelper assemblyHelper, IAbilityManager abilityManager)
        {
            // Get classes using reflection
            Type iClassType = typeof(IClass);
            _classes = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iClassType.IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x, abilityManager)) // don't use DependencyContainer
                .OfType<IClass>())
                .ToList();
        }

        #region IClassManager

        public IEnumerable<IClass> Classes => _classes;

        public IClass this[string name] => _classes.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));

        #endregion
    }
}
