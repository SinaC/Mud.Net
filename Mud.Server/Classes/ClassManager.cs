using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Server.Helpers;

namespace Mud.Server.Classes
{
    public class ClassManager : IClassManager
    {
        private readonly List<IClass> _classes;

        #region Singleton

        private static readonly Lazy<ClassManager> Lazy = new Lazy<ClassManager>(() => new ClassManager());

        public static IClassManager Instance => Lazy.Value;

        private ClassManager()
        {
            // Get classes using reflection
            Type iClassType = typeof(IClass);
            _classes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && iClassType.IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<IClass>()
                .ToList();
        }

        #endregion

        #region IClassManager

        public IEnumerable<IClass> Classes => _classes;

        public IClass this[string name] => _classes.FirstOrDefault(x => FindHelpers.StringEquals(x.Name, name));

        #endregion
    }
}
