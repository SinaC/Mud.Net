using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect;
using System.Reflection;

namespace Mud.Server.Affects;

[Export(typeof(IAffectManager)), Shared]
public class AffectManager : IAffectManager
{
    private ILogger<AffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    private Dictionary<string, AffectDefinition> AffectDefinitionByName { get; }
    private Dictionary<Type, AffectDefinition> AffectDefinitionByDataType { get; }

    public AffectManager(ILogger<AffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        var iAffectType = typeof(IAffect);
        var affectDataBaseType = typeof(AffectDataBase);
        var affectDefinitions = new List<AffectDefinition>();
        foreach (var affectType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(iAffectType))))
        {
            var affectAttribute = affectType.GetCustomAttribute<AffectAttribute>() ?? throw new Exception($"AffectManager: no AffectAttribute found for Affect {affectType.FullName}");
            var isAffectDataTypeValid = affectAttribute.AffectDataType.IsAssignableTo(affectDataBaseType);
            if (!isAffectDataTypeValid)
                throw new Exception($"AffectManager: AffectData type {affectAttribute.AffectDataType} doesn't inherit from {affectDataBaseType.FullName} on Affect {affectType.FullName}");
            var initializeMethod = affectType.SearchMethod("Initialize", affectAttribute.AffectDataType) ?? throw new Exception($"AffectManager: no valid Initialize(affect data) method found on Affect {affectType.FullName}");
            var affectDefinition = new AffectDefinition(affectType, affectAttribute.Name, affectAttribute.AffectDataType, initializeMethod);
            affectDefinitions.Add(affectDefinition);
        }
        AffectDefinitionByName = affectDefinitions.ToDictionary(x => x.Name, x => x);
        AffectDefinitionByDataType = affectDefinitions.ToDictionary(x => x.AffectDataType, x => x);
    }

    public IAffect? CreateInstance(string name)
    {
        if (!AffectDefinitionByName.TryGetValue(name, out var affectDefinition))
        {
            Logger.LogError("AffectManager: affect {name} not found.", name);
            return null;
        }

        return CreateInstance(affectDefinition);
    }

    public IAffect? CreateInstance(AffectDataBase affectData)
    {
        var affectDataType = affectData.GetType();
        if (!AffectDefinitionByDataType.TryGetValue(affectDataType, out var affectDefinition))
        {
            Logger.LogError("AffectManager: unexpected AffectData type {affectDataType}.", affectDataType.FullName);
            return null;
        }
        // create instance
        var affect = CreateInstance(affectDefinition);
        if (affect == null)
        {
            Logger.LogError("AffectManager: cannot create instance of Affect {affectType}", affectDefinition.AffectType.FullName);
            return null;
        }
        affectDefinition.InitializeMethod.Invoke(affect, [affectData]);
        return affect;
    }

    private IAffect? CreateInstance(AffectDefinition affectDefinition)
    {
        var affect = ServiceProvider.GetRequiredService(affectDefinition.AffectType);
        if (affect is not IAffect instance)
        {
            Logger.LogError("AffectManager: affect {affectType} cannot be created or is not of type {expectedAffectType}", affectDefinition.AffectType.FullName ?? "???", typeof(IAffect).FullName ?? "???");
            return null;
        }
        return instance;
    }
}
