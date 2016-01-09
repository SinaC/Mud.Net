using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Abilities
{
    public class Abilities
    {
        private readonly Dictionary<string, IAbility> _abilities;

        public IReadOnlyDictionary<string, IAbility> List
        {
            get { return _abilities; }
        }

        #region Singleton

        private static readonly Lazy<Abilities> Lazy = new Lazy<Abilities>(() => new Abilities());

        public static Abilities Instance
        {
            get { return Lazy.Value; }
        }

        private Abilities()
        {
            Type abilityType = typeof(IAbility);
            _abilities = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => abilityType.IsAssignableFrom(x) && !x.IsAbstract)
                .Select(Activator.CreateInstance).OfType<IAbility>()
                .ToDictionary(x => x.Name);
        }

        #endregion
    }
}
