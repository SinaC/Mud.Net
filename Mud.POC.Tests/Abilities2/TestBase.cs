using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public abstract class TestBase
    {
        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }
    }
}
