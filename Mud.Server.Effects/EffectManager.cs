using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using System.Reflection;

namespace Mud.Server.Effects;

public class EffectManager : IEffectManager
{
    private ILogger<EffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, Type> EffectsByName { get; }

    public EffectManager(ILogger<EffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        Type iEffectType = typeof(IEffect);
        EffectsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iEffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<EffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.Name, x => x.executionType);
    }

    public IEffect<TEntity>? CreateInstance<TEntity>(string effectName)
        where TEntity : IEntity
    {
        if (!EffectsByName.TryGetValue(effectName, out var effectType))
        {
            Logger.LogError("EffectManager: effect {effectName} not found.", effectName);
            return null;
        }

        var effect = ServiceProvider.GetService(effectType);
        if (effect == null)
        {
            Logger.LogError("EffectManager: effect {effectType} not found in DependencyContainer.", effectType.FullName ?? "???");
            return null;
        }

        if (effect is not IEffect<TEntity> instance)
        {
            Logger.LogError("EffectManager: effect {effectType} cannot be created or is not of type {expectedEffectType}", effectType.FullName ?? "???", typeof(IEffect<TEntity>).FullName ?? "???");
            return null;
        }

        return instance;
    }
}
