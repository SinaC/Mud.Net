using Mud.Container;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Effects
{
    public class EffectManager : IEffectManager
    {
        public Dictionary<string, Type> _effectByNames;

        public EffectManager(IAssemblyHelper assemblyHelper)
        {
            Type iEffectType = typeof(IEffect);
            _effectByNames = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iEffectType.IsAssignableFrom(t)))
                .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<EffectAttribute>() })
                .Where(x => x.attribute != null)
                .ToDictionary(x => x.attribute.Name, x => x.executionType);
        }

        public IEffect<TEntity> CreateInstance<TEntity>(string name)
            where TEntity : IEntity
        {
            if (!_effectByNames.TryGetValue(name, out var effectType))
            {
                Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} not found.", name);
                return null;
            }

            if (DependencyContainer.Current.GetRegistration(effectType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} not found in DependencyContainer.", effectType.FullName);
                return null;
            }

            IEffect<TEntity> instance = DependencyContainer.Current.GetInstance(effectType) as IEffect<TEntity>;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} cannot be create or is not of type {1}", effectType.FullName, typeof(IEffect<TEntity>).FullName);
                return null;
            }
            return instance;
        }
    }
}
