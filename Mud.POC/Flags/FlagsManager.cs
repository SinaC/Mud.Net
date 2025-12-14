using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using System.Reflection;
using System.Text;

namespace Mud.POC.Flags
{
    [Export(typeof(IFlagsManager)), Shared]
    public class FlagsManager : IFlagsManager
    {
        private ILogger<FlagsManager> Logger { get; }

        private Dictionary<Type, Dictionary<string, FlagDefinition>> FlagDefinitionsByFlagType { get; }

        public FlagsManager(ILogger<FlagsManager> logger, IEnumerable<IFlagValues> defineFlagValues)
        {
            Logger = logger;

            FlagDefinitionsByFlagType = [];
            var iFlagsStringType = typeof(IFlags<string>);
            foreach (var defineFlagValue in defineFlagValues)
            {
                var defineFlagValueType = defineFlagValue.GetType();
                var defineFlagsAttribute = defineFlagValueType.GetCustomAttribute<DefineFlagValuesAttribute>() ?? throw new Exception($"FlagManager: no DefineFlagValuesAttribute found for DefineFlagValues {defineFlagValueType.FullName}");
                var flagInterfaceType = defineFlagsAttribute.FlagInterfaceType;
                if (!flagInterfaceType.IsAssignableTo(iFlagsStringType))
                    throw new Exception($"FlagManager: FlagInterfaceType {flagInterfaceType.FullName} in DefineFlagValues {defineFlagValueType.FullName} doesn't inherit from {iFlagsStringType.FullName}");

                var isFlagTypeDefined = FlagDefinitionsByFlagType.TryGetValue(flagInterfaceType, out var flagDefinitions);
                if (!isFlagTypeDefined || flagDefinitions == null)
                {
                    flagDefinitions = new Dictionary<string, FlagDefinition>(StringComparer.InvariantCultureIgnoreCase);
                    FlagDefinitionsByFlagType.Add(flagInterfaceType, flagDefinitions);
                }

                foreach (var availableFlag in defineFlagValue.AvailableFlags)
                {
                    if (flagDefinitions.ContainsKey(availableFlag))
                        Logger.LogError("FlagManager: Flag {availableFlag} found in DefineFlagValues {defineFlagValueType} is already defined", availableFlag, defineFlagValueType.FullName);
                    else
                    {
                        var flagDefinition = new FlagDefinition(availableFlag, defineFlagValue);
                        flagDefinitions.Add(availableFlag, flagDefinition);
                    }
                }
            }
        }

        public bool CheckFlags<TFlags>(TFlags flags)
            where TFlags : IFlags<string>
        {
            var tFlagsType = typeof(TFlags);
            if (!FlagDefinitionsByFlagType.TryGetValue(tFlagsType, out var flagDefinitions))
            {
                Logger.LogError("FlagManager: unknown type {flagType}", tFlagsType.FullName);
                return false;
            }
            var invalidValues = flags.Values.Except(flagDefinitions.Keys).ToArray();
            if (invalidValues.Length == 0)
                return true;
            Logger.LogError("FlagManager: '{invalidValues}' not found in {type}", string.Join(",", invalidValues), tFlagsType.FullName);
            return false;
        }

        public StringBuilder Append<TFlags>(StringBuilder sb, TFlags flags, bool shortDisplay)
            where TFlags : IFlags<string>
        {
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
                    var flagPrettyPrint = flagDefinition.DefineFlagValues.PrettyPrint(flag, shortDisplay);
                    if (!string.IsNullOrWhiteSpace(flagPrettyPrint))
                        sb.Append(flagPrettyPrint);
                }
                else
                    // no append
                    Logger.LogError("FlagManager: flag {flag} not found in {type}", flag, tFlagsType.FullName);
            }
            return sb;
        }
    }
}
