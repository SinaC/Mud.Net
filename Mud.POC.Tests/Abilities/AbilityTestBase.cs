using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.POC.Tests.Abilities
{
    public abstract class AbilityTestBase
    {
        protected (string rawParameters, ICommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected (string rawParameters, ICommandParameter[] parameters) BuildParametersSkipFirst(string parameters)
        {
            return CommandHelpers.SkipParameters(CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter), 1);
        }
    }
}
