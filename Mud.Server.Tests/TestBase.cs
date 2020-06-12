using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Tests.Mocking;
using Mud.Settings;

namespace Mud.Server.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        protected IWorld World => Container.DependencyContainer.Current.GetInstance<IWorld>();
        protected IRoomManager RoomManager => Container.DependencyContainer.Current.GetInstance<IRoomManager>();
        protected IItemManager ItemManager => Container.DependencyContainer.Current.GetInstance<IItemManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            (Container.DependencyContainer.Current.GetInstance<IWorld>() as WorldMock).Clear();
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Container.DependencyContainer.Current.RegisterInstance<ISettings>(new SettingsMock());
            Container.DependencyContainer.Current.RegisterInstance<ITimeManager>(new TimeHandlerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRaceManager>(new RaceManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IClassManager>(new ClassManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IWorld>(new WorldMock());
            Container.DependencyContainer.Current.RegisterInstance<IWiznet>(new WiznetMock());
            Container.DependencyContainer.Current.RegisterInstance<IRoomManager>(new RoomManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IItemManager>(new ItemManagerMock());
        }
    }
}
