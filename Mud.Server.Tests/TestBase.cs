using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Tests.Mocking;
using Mud.Settings;

namespace Mud.Server.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Container.DependencyContainer.Current.RegisterInstance<ISettings>(new SettingsMock());
            Container.DependencyContainer.Current.RegisterInstance<ITimeHandler>(new TimeHandlerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRaceManager>(new RaceManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IClassManager>(new ClassManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManagerMock());
        }
    }
}
