using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Server.Common;

namespace Mud.Server.Classes
{
    public class ClassManager : IClassManager
    {
        private readonly List<IClass> _classes;

        public ClassManager()
        {
            // Get classes using reflection
            Type iClassType = typeof(IClass);
            _classes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iClassType.IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<IClass>()
                .ToList();
        }

        #region IClassManager

        public IEnumerable<IClass> Classes => _classes;

        public IClass this[string name] => _classes.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name));

        #endregion
    }
}
