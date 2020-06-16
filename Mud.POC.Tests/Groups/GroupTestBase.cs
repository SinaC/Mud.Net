using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public abstract class GroupTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            World.Instance.Clear();
        }

        protected (string rawParameters, ICommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected (string rawParameters, ICommandParameter[] parameters) BuildParametersSkipFirst(string parameters)
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
