using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;

namespace Mud.Server.Tests.Rom24
{
    [TestClass]
    public class Rom24SpellTests : TestBase
    {
        [TestMethod]
        public void TestAcidBlast()
        {
            // TODO
            var instance = DependencyContainer.Current.GetInstance<Abilities.Rom24.Rom24Spells>();
        }
    }
}
