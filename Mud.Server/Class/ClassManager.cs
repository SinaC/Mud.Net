using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Common;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class
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
