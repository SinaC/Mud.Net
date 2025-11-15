using Mud.Container;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using System.Reflection;

namespace Mud.Server.Effects;

public class EffectManager : IEffectManager
{
    private readonly Dictionary<string, Type> _effectsByName;

    public EffectManager(IAssemblyHelper assemblyHelper)
    {
        Type iEffectType = typeof(IEffect);
        _effectsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iEffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<EffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.Name, x => x.executionType);
    }

    public IEffect<TEntity>? CreateInstance<TEntity>(string effectName)
        where TEntity : IEntity
    {
        if (!_effectsByName.TryGetValue(effectName, out var effectType))
        {
            Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} not found.", effectName);
            return null;
        }

        if (DependencyContainer.Current.GetRegistration(effectType, false) == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} not found in DependencyContainer.", effectType.FullName ?? "???");
            return null;
        }

        if (DependencyContainer.Current.GetInstance(effectType) is not IEffect<TEntity> instance)
        {
            Log.Default.WriteLine(LogLevels.Error, "EffectManager: effect {0} cannot be created or is not of type {1}", effectType.FullName ?? "???", typeof(IEffect<TEntity>).FullName ?? "???");
            return null;
        }

        return instance;
    }
}
