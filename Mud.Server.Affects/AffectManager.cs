using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Affects.Room;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect;
using System.Reflection;

namespace Mud.Server.Affects;

public class AffectManager : IAffectManager
{
    private ILogger<AffectManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    private Dictionary<string, AffectInfo> AffectsByName { get; }
    private Dictionary<Type, AffectInfo> AffectsByDataType { get; }

    public AffectManager(ILogger<AffectManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;

        var iAffectType = typeof(IAffect);
        var affectInfos = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iAffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<AffectAttribute>()! })
            .Where(x => x.attribute != null)
            .Select(x => new AffectInfo(x.executionType, x.attribute.Name, x.attribute.AffectDataType, x.attribute))
            .ToArray();
        AffectsByName = affectInfos.ToDictionary(x => x.Name, x => x);
        AffectsByDataType = affectInfos.ToDictionary(x => x.AffectDataType, x => x);
    }

    public IAffect? CreateInstance(string name)
    {
        if (!AffectsByName.TryGetValue(name, out var affectInfo))
        {
            Logger.LogError("AffectManager: effect {name} not found.", name);
            return null;
        }

        return CreateInstance(affectInfo);
    }

    public IAffect? CreateInstance(AffectDataBase data)
    {
        switch (data)
        {
            case CharacterAttributeAffectData characterAttributeAffectData:
                return new CharacterAttributeAffect(characterAttributeAffectData);
            case CharacterFlagsAffectData characterFlagsAffectData:
                return new CharacterFlagsAffect(characterFlagsAffectData);
            case CharacterIRVAffectData characterIRVAffectData:
                return new CharacterIRVAffect(characterIRVAffectData);
            case CharacterSexAffectData characterSexAffectData:
                return new CharacterSexAffect(characterSexAffectData);
            case ItemFlagsAffectData itemFlagsAffectData:
                return new ItemFlagsAffect(itemFlagsAffectData);
            case ItemWeaponFlagsAffectData itemWeaponFlagsAffectData:
                return new ItemWeaponFlagsAffect(itemWeaponFlagsAffectData);
            case RoomFlagsAffectData roomFlagsAffectData:
                return new RoomFlagsAffect(roomFlagsAffectData);
            case RoomHealRateAffectData roomHealRateAffectData:
                return new RoomHealRateAffect(roomHealRateAffectData);
            case RoomResourceRateAffectData roomResourceRateAffectData:
                return new RoomResourceRateAffect(roomResourceRateAffectData);
            default:
                var dataType = data.GetType();
                if (!AffectsByDataType.TryGetValue(dataType, out var affectInfo))
                {
                    Logger.LogError("Unexpected AffectType type {dataType}.", data.GetType());
                }
                else
                {
                    var affect = CreateInstance(affectInfo);
                    if (affect is ICustomAffect customAffect)
                        customAffect.Initialize(data);
                    else
                        Logger.LogError("AffectType type {dataType} should implement {customAffectType}.", data.GetType(), typeof(ICustomAffect).FullName ?? "???");
                    return affect;
                }
                break;
        }

        return null;
    }

    private IAffect? CreateInstance(AffectInfo affectInfo)
    {
        var affect = ServiceProvider.GetService(affectInfo.AffectType);
        if (affect == null)
        {
            Logger.LogError("AffectManager: affect {affectType} not found in DependencyContainer.", affectInfo.AffectType.FullName ?? "???");
            return null;
        }

        if (affect is not IAffect instance)
        {
            Logger.LogError("AffectManager: affect {affectType} cannot be create or is not of type {expectedAffectType}", affectInfo.AffectType.FullName ?? "???", typeof(IAffect).FullName ?? "???");
            return null;
        }
        return instance;
    }
}
