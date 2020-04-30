using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Common;
using Mud.Server.Tests.Mocking;
using Mud.Settings;

namespace Mud.Server.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        protected IWorld World => Container.DependencyContainer.Current.GetInstance<IWorld>();

        [TestInitialize]
        public void TestInitialize()
        {
            (Container.DependencyContainer.Current.GetInstance<IWorld>() as WorldMock).Clear();
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Container.DependencyContainer.Current.RegisterInstance<ISettings>(new SettingsMock());
            Container.DependencyContainer.Current.RegisterInstance<ITimeHandler>(new TimeHandlerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRaceManager>(new RaceManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IClassManager>(new ClassManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IWorld>(new WorldMock());
            Container.DependencyContainer.Current.RegisterInstance<IWiznet>(new WiznetMock());
            Container.DependencyContainer.Current.RegisterInstance<IRandomManager>(new RandomManager());
        }
    }
}
