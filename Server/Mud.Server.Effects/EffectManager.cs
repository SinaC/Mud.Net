using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Entity;
using System.Reflection;

namespace Mud.Server.Effects;

[Export(typeof(IEffectManager)), Shared]
public class EffectManager : IEffectManager
{
    private ILogger<EffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, Type> EffectsByName { get; }

    public EffectManager(ILogger<EffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        var iEffectType = typeof(IEffect);
        EffectsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iEffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<EffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.Name, x => x.executionType);
    }

    public int Count => EffectsByName.Count;

    public IEffect<TEntity>? CreateInstance<TEntity>(string effectName)
        where TEntity : IEntity
    {
        if (!EffectsByName.TryGetValue(effectName, out var effectType))
        {
            Logger.LogError("EffectManager: effect {effectName} not found.", effectName);
            return null;
        }

        var effect = ServiceProvider.GetRequiredService(effectType);
        if (effect is not IEffect<TEntity> instance)
        {
            Logger.LogError("EffectManager: effect {effectType} cannot be created or is not of type {expectedEffectType}", effectType.FullName ?? "???", typeof(IEffect<TEntity>).FullName ?? "???");
            return null;
        }

        return instance;
    }
}
