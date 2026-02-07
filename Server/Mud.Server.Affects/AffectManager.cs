using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect;
using System.Reflection;

namespace Mud.Server.Affects;

[Export(typeof(IAffectManager)), Shared]
public class AffectManager : IAffectManager
{
    private ILogger<AffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    private Dictionary<string, AffectDefinition> AffectDefinitionByName { get; }
    private Dictionary<Type, AffectDefinition> AffectDefinitionByAffectDataType { get; }

    public AffectManager(ILogger<AffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        // for each affect
        //      if it's a NoAffectData affect, dont try to search Initialize method and dont store it in AffectDefinitionByAffectDataType
        //      else, search Initialize(TAffectData) method and store it in AffectDefinitionByAffectDataType
        var iAffectType = typeof(IAffect);
        var affectDataBaseType = typeof(AffectDataBase);
        var noAffectDataType = typeof(NoAffectData);
        var affectDefinitions = new List<AffectDefinition>();
        foreach (var affectType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(iAffectType))))
        {
            var affectAttribute = affectType.GetCustomAttribute<AffectAttribute>() ?? throw new Exception($"AffectManager: no AffectAttribute found for Affect {affectType.FullName}");
            var affectDataType = affectAttribute.AffectDataType;
            if (affectDataType.IsAssignableTo(noAffectDataType))
            {
                var affectDefinition = new AffectDefinition(affectType, affectAttribute.Name);
                affectDefinitions.Add(affectDefinition);
            }
            else
            {
                var isAffectDataTypeValid = affectDataType.IsAssignableTo(affectDataBaseType);
                if (!isAffectDataTypeValid)
                    throw new Exception($"AffectManager: AffectData type {affectDataType} doesn't inherit from {affectDataBaseType.FullName} on Affect {affectType.FullName}");
                var initializeMethod = affectType.SearchMethod("Initialize", affectDataType) ?? throw new Exception($"AffectManager: no valid Initialize(affect data) method found on Affect {affectType.FullName}");
                var affectDefinition = new AffectDefinition(affectType, affectAttribute.Name, affectDataType, initializeMethod);
                affectDefinitions.Add(affectDefinition);
            }
        }
        AffectDefinitionByName = affectDefinitions.ToDictionary(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        AffectDefinitionByAffectDataType = affectDefinitions.Where(x => !x.NoAffectData).ToDictionary(x => x.AffectDataType!);
    }

    public int Count => AffectDefinitionByName.Count;

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
        // if NoAffectData, use affectName to search affect definition and create instance
        if (affectData is NoAffectData noAffectData)
            return CreateInstance(noAffectData.AffectName);
        // if not NoAffectData, use affectDataType to search affect definition and create instance
        var affectDataType = affectData.GetType();
        if (!AffectDefinitionByAffectDataType.TryGetValue(affectDataType, out var affectDefinition))
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
        affectDefinition.InitializeMethod!.Invoke(affect, [affectData]);
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
