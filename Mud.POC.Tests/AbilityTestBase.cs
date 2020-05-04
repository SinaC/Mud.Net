
using System.Linq;
using Mud.Server.Input;

namespace Mud.POC.Tests
{
    public abstract class AbilityTestBase
    {
        protected (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }
    }
}
