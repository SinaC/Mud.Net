using System.Linq;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;

namespace Mud.POC.Tests.Groups
{
    public abstract class GroupTestBase
    {
        protected (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected (string rawParameters, CommandParameter[] parameters) BuildParametersSkipFirst(string parameters)
        {
            return CommandHelpers.SkipParameters(CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter), 1);
        }

        protected IGroup CreateGroup(IPlayableCharacter leader, params IPlayableCharacter[] members)
        {
            foreach (IPlayableCharacter member in members)
            {
                var groupArgs = BuildParameters(member.Name);
                leader.DoGroup(groupArgs.rawParameters, groupArgs.parameters);
            }

            return leader.Group;
        }
    }
}
