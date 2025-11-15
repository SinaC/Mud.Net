using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect;
using System.Reflection;

namespace Mud.Server.Affects;

public class AffectManager : IAffectManager
{
    private readonly Dictionary<string, IAffectInfo> _affectsByName;

    public AffectManager(IAssemblyHelper assemblyHelper)
    {
        Type iAffectType = typeof(IAffect);
        _affectsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iAffectType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<AffectAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.Name, x => CreateAffectInfo(x.executionType, x.attribute));
    }

    public IAffect? CreateInstance(string name)
    {
        if (!_affectsByName.TryGetValue(name, out var affectInfo))
        {
            Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} not found.", name);
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
                Type dataType = data.GetType();
                var affectInfo = _affectsByName.Values.FirstOrDefault(x => x.AffectDataType == dataType);
                if (affectInfo != null)
                {
                    var affect = CreateInstance(affectInfo);
                    if (affect is ICustomAffect customAffect)
                        customAffect.Initialize(data);
                    else
                        Log.Default.WriteLine(LogLevels.Error, "AffectType type {0} should implement {1}.", data.GetType(), typeof(ICustomAffect).FullName ?? "???");
                    return affect;
                }
                else
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected AffectType type {0}.", data.GetType());
                break;
        }

        return null;
    }

    private static IAffectInfo CreateAffectInfo(Type executionType, AffectAttribute attribute)
        => new AffectInfo(executionType, attribute.Name, attribute.AffectDataType);

    private static IAffect? CreateInstance(IAffectInfo affectInfo)
    {
        if (DependencyContainer.Current.GetRegistration(affectInfo.AffectType, false) == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} not found in DependencyContainer.", affectInfo.AffectType.FullName ?? "???");
            return null;
        }

        if (DependencyContainer.Current.GetInstance(affectInfo.AffectType) is not IAffect instance)
        {
            Log.Default.WriteLine(LogLevels.Error, "AffectManager: effect {0} cannot be create or is not of type {1}", affectInfo.AffectType.FullName ?? "???", typeof(IAffect).FullName ?? "???");
            return null;
        }
        return instance;
    }
}
