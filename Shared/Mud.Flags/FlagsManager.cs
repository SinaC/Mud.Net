using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Flags.Interfaces;
using System.Reflection;
using System.Text;

namespace Mud.Flags;

[Export(typeof(IFlagsManager)), Shared]
public class FlagsManager : IFlagsManager
{
    private ILogger<FlagsManager> Logger { get; }

    private Dictionary<Type, Dictionary<string, FlagDefinition>> FlagDefinitionsByFlagType { get; }

    public FlagsManager(ILogger<FlagsManager> logger, IEnumerable<IFlagValues> flagValuesInstances)
    {
        Logger = logger;

        FlagDefinitionsByFlagType = [];
        var iFlagsStringType = typeof(IFlags<string>);
        foreach (var flagValuesInstance in flagValuesInstances)
        {
            var flagValuesType = flagValuesInstance.GetType();
            var flagValuesAttribute = flagValuesType.GetCustomAttribute<FlagValuesAttribute>() ?? throw new Exception($"FlagManager: no FlagValuesAttribute found for FlagValues {flagValuesType.FullName}");
            var flagInterfaceType = flagValuesAttribute.FlagInterfaceType;
            if (!flagInterfaceType.IsAssignableTo(iFlagsStringType))
                throw new Exception($"FlagManager: FlagInterfaceType {flagInterfaceType.FullName} in FlagValues {flagValuesType.FullName} doesn't inherit from {iFlagsStringType.FullName}");

            var isFlagTypeDefined = FlagDefinitionsByFlagType.TryGetValue(flagInterfaceType, out var flagDefinitions);
            if (!isFlagTypeDefined || flagDefinitions == null)
            {
                flagDefinitions = new Dictionary<string, FlagDefinition>(StringComparer.InvariantCultureIgnoreCase);
                FlagDefinitionsByFlagType.Add(flagInterfaceType, flagDefinitions);
            }

            foreach (var availableFlag in flagValuesInstance.AvailableFlags)
            {
                if (flagDefinitions.ContainsKey(availableFlag))
                    Logger.LogError("FlagManager: Flag {availableFlag} found in FlagValues {flagValuesType} is already defined", availableFlag, flagValuesType.FullName);
                else
                {
                    var flagDefinition = new FlagDefinition(availableFlag, flagValuesInstance);
                    flagDefinitions.Add(availableFlag, flagDefinition);
                }
            }
        }
    }


    public bool CheckFlags(Type flagType, IFlags<string>? flags)
    {
        if (flags == null)
            return true;
        if (!FlagDefinitionsByFlagType.TryGetValue(flagType, out var flagDefinitions))
        {
            Logger.LogError("FlagManager: unknown type {flagType}", flagType.FullName);
            return false;
        }
        var invalidValues = flags.Values.Except(flagDefinitions.Keys, StringComparer.InvariantCultureIgnoreCase).ToArray();
        if (invalidValues.Length == 0)
            return true;
        Logger.LogError("FlagManager: '{invalidValues}' not found in {type}", string.Join(",", invalidValues), flagType.FullName);
        return false;
    }

    public bool CheckFlags<TFlags>(TFlags? flags)
        where TFlags : IFlags<string>
        => CheckFlags(typeof(TFlags), flags);

    public StringBuilder Append<TFlags>(StringBuilder sb, TFlags? flags, bool shortDisplay)
        where TFlags : IFlags<string>
    {
        if (flags == null)
            return sb;
        var tFlagsType = typeof(TFlags);
        if (!FlagDefinitionsByFlagType.TryGetValue(tFlagsType, out var flagDefinitions))
        {
            Logger.LogError("FlagManager: unknown type {flagType}", tFlagsType.FullName);
            // no append
            return sb;
        }
        foreach (var flag in flags.Values)
        {
            if (flagDefinitions.TryGetValue(flag, out var flagDefinition))
            {
                var flagPrettyPrint = flagDefinition.FlagValues.PrettyPrint(flag, shortDisplay);
                if (!string.IsNullOrWhiteSpace(flagPrettyPrint))
                    sb.Append(flagPrettyPrint);
            }
            else
                // no append
                Logger.LogError("FlagManager: flag {flag} not found in {type}", flag, tFlagsType.FullName);
        }
        return sb;
    }
    private class FlagDefinition
    {
        public string Flag { get; }
        public IFlagValues FlagValues { get; }

        public FlagDefinition(string flag, IFlagValues flagValues)
        {
            Flag = flag;
            FlagValues = flagValues;
        }
    }
}
